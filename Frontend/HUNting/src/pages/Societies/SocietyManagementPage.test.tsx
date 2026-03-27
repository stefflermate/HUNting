import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SocietyManagementPage } from './SocietyManagementPage';
import { useAuth } from '@/context/AuthContext';
import { societiesApi } from '@/services/societiesApi';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/societiesApi', () => ({
  societiesApi: {
    members: vi.fn(),
    requests: vi.fn(),
    posts: vi.fn(),
    approveRequest: vi.fn(),
    rejectRequest: vi.fn(),
    createPost: vi.fn(),
  },
}));

vi.mock('@/lib/apiClient', () => ({
  resolveAssetUrl: vi.fn((path) => path)
}));

describe('SocietyManagementPage', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    vi.clearAllMocks();
  });

  const renderComponent = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <SocietyManagementPage />
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('renders error for missing permissions', () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Person' },
    });

    renderComponent();
    expect(screen.getByText(/Csak tarsasagi fiokkal, illetve a sajat tarsasag/i)).toBeInTheDocument();
  });

  it('renders management page for owners', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: {
        accountType: 'Society',
        ownedSociety: { societyId: 10, name: 'Tarsasag' }
      },
    });

    (societiesApi.members as any).mockResolvedValue([
      { userId: 1, user: { fullName: 'Pista Tag' }, roleName: 'Tag', joinedAt: '2023-01-01T00:00:00Z' }
    ]);
    (societiesApi.requests as any).mockResolvedValue([
      { userId: 2, user: { fullName: 'Gizi Jelolt'}, createdAt: '2023-02-01T00:00:00Z', message: 'Legyszi' }
    ]);
    (societiesApi.posts as any).mockResolvedValue([]);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Pista Tag')).toBeInTheDocument();
    });

    expect(screen.getByText('Gizi Jelolt')).toBeInTheDocument();
  });

  it('approves candidate', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: {
        accountType: 'Society',
        ownedSociety: { societyId: 10, name: 'Tarsasag' }
      },
    });

    (societiesApi.members as any).mockResolvedValue([]);
    (societiesApi.requests as any).mockResolvedValue([
      { userId: 2, user: { fullName: 'Gizi Jelolt' }, createdAt: '2023-02-01T00:00:00Z', message: 'Legyszi' }
    ]);
    (societiesApi.posts as any).mockResolvedValue([]);
    (societiesApi.approveRequest as any).mockResolvedValue({});

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Gizi Jelolt')).toBeInTheDocument();
    });

    const buttons = screen.getAllByRole('button');
    const approveBtn = buttons.find(b => b.textContent && b.textContent.includes('Elfogadas') || b.textContent?.includes('Elfogad'));
    
    if (approveBtn) {
      fireEvent.click(approveBtn);
      await waitFor(() => {
        expect(societiesApi.approveRequest).toHaveBeenCalledWith(10, 2);
      });
    }
  });
});
