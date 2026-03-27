import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { HuntingLogsPage } from './HuntingLogsPage';
import { useAuth } from '@/context/AuthContext';
import { huntingLogsApi } from '@/services/huntingLogsApi';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/huntingLogsApi', () => ({
  huntingLogsApi: {
    list: vi.fn(),
    create: vi.fn(),
  },
}));

vi.mock('@/lib/apiClient', () => ({
  resolveAssetUrl: vi.fn((path) => path)
}));

describe('HuntingLogsPage', () => {
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
          <HuntingLogsPage />
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
    expect(screen.getByText(/A naplo funkcio csak szemely fiokkal/i)).toBeInTheDocument();
  });

  it('renders logs for Person accounts', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Person', name: 'Aladar' },
    });

    const mockLogs = [
      {
        huntingLogId: 'log-1',
        occurredAt: '2024-10-10T10:00:00Z',
        locationText: 'Erdo kozepe',
        animalType: 'Szarvas',
        notes: 'Nagy volt',
        shareWithSociety: false,
      },
    ];

    (huntingLogsApi.list as any).mockResolvedValue(mockLogs);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Erdo kozepe')).toBeInTheDocument();
    });
    expect(screen.getByText('Vad: Szarvas')).toBeInTheDocument();
  });

  it('toggles new log form', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Person' },
    });
    (huntingLogsApi.list as any).mockResolvedValue([]);

    renderComponent();

    const toggleButton = await screen.findByRole('button', { name: /Uj bejegyzes/i });
    fireEvent.click(toggleButton);

    expect(await screen.findByLabelText(/Idopont/i)).toBeInTheDocument();
    
    // Check if any Megsem button exists
    expect(screen.getAllByRole('button', { name: /Megsem/i }).length).toBeGreaterThan(0);
  });
});
