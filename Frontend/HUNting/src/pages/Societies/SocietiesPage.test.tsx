import { render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SocietiesPage } from './SocietiesPage';
import { societiesApi } from '@/services/societiesApi';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';

vi.mock('@/services/societiesApi', () => ({
  societiesApi: {
    list: vi.fn(),
  },
}));

describe('SocietiesPage', () => {
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
          <SocietiesPage />
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('renders societies list successfully', async () => {
    const mockSocieties = [
      {
        societyId: 'soc-1',
        name: 'Bakonyszombathelyi Vadasztarsasag',
        description: 'Bakonyszombathelyi jo vadaszok',
        memberCount: 42,
        createdAt: '2023-01-01T00:00:00Z',
      },
    ];

    (societiesApi.list as any).mockResolvedValue(mockSocieties);

    renderComponent();

    expect(screen.getByText(/Közösségi névsor/i)).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('Bakonyszombathelyi Vadasztarsasag')).toBeInTheDocument();
    });

    expect(screen.getByText('42 tag')).toBeInTheDocument();
    expect(screen.getByText('Bakonyszombathelyi jo vadaszok')).toBeInTheDocument();
  });

  it('renders loading skeletons on fetch', () => {
    // create a never-resolving promise to stay in fetching state
    (societiesApi.list as any).mockImplementation(() => new Promise(() => {}));

    const { container } = renderComponent();
    expect(screen.getByText(/Közösségi névsor/i)).toBeInTheDocument();
    // Verify skeletons exist using the animate-pulse class
    expect(container.getElementsByClassName('animate-pulse').length).toBe(4);
  });
});
