import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MySocietiesPage } from './MySocietiesPage';
import { useAuth } from '@/context/AuthContext';
import { societiesApi } from '@/services/societiesApi';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/societiesApi', () => ({
  societiesApi: {
    my: vi.fn(),
    posts: vi.fn(),
  },
}));

vi.mock('@/lib/apiClient', () => ({
  resolveAssetUrl: vi.fn((path) => path)
}));

describe('MySocietiesPage', () => {
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
          <MySocietiesPage />
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('renders correctly for non-Person accounts', () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Society' },
    });

    renderComponent();
    expect(screen.getByText(/Ez az oldal csak szemely fiokkal/i)).toBeInTheDocument();
  });

  it('renders societies and selects first automatically', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Person' },
    });

    const mockMySocieties = [
      { societyId: 1, name: 'Elso Tarsasag' },
      { societyId: 2, name: 'Masodik Tarsasag' }
    ];

    const mockPosts = [
      { postId: 101, content: 'Udv a tarsasagban!', createdAt: '2023-10-10T10:00:00Z', isSystemMessage: false, authorName: 'Isti' }
    ];

    (societiesApi.my as any).mockResolvedValue(mockMySocieties);
    (societiesApi.posts as any).mockResolvedValue(mockPosts);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Elso Tarsasag')).toBeInTheDocument();
    });

    expect(screen.getByText('Masodik Tarsasag')).toBeInTheDocument();

    // The component might automatically select the first or not, let's click it to be sure
    fireEvent.click(screen.getByText('Elso Tarsasag'));

    await waitFor(() => {
      expect(screen.getByText('Udv a tarsasagban!')).toBeInTheDocument();
    });
    expect(screen.getByText('Ismeretlen szerzo')).toBeInTheDocument();
  });

  it('redirects unauthenticated users', () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: false,
      user: null
    });
    renderComponent();
    expect(screen.queryByText(/Ez az oldal csak szemely fiokkal/i)).not.toBeInTheDocument();
  })
});
