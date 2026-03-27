import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SocietyDetailPage } from './SocietyDetailPage';
import { useAuth } from '@/context/AuthContext';
import { societiesApi } from '@/services/societiesApi';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/societiesApi', () => ({
  societiesApi: {
    getById: vi.fn(),
    join: vi.fn(),
  },
}));

describe('SocietyDetailPage', () => {
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
        <MemoryRouter initialEntries={['/societies/10']}>
          <Routes>
            <Route path="/societies/:societyId" element={<SocietyDetailPage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('renders society details successfully', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Person' },
    });

    (societiesApi.getById as any).mockResolvedValue({
      societyId: 10,
      name: 'Nagy Vadasz Klub',
      description: 'Mi vagyunk a legjobbak',
      memberCount: 50,
      email: 'klub@vadasz.hu',
      createdAt: '2023-01-01T00:00:00Z',
    });

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Nagy Vadasz Klub')).toBeInTheDocument();
    });

    expect(screen.getByText('50')).toBeInTheDocument();
    expect(screen.getByText('klub@vadasz.hu')).toBeInTheDocument();
  });

  it('allows Person to send a join request', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Person' },
    });

    (societiesApi.getById as any).mockResolvedValue({
      societyId: 10,
      name: 'Nagy Vadasz Klub',
      memberCount: 50,
      email: 'klub@vadasz.hu',
      createdAt: '2023-01-01T00:00:00Z',
    });

    (societiesApi.join as any).mockResolvedValue({});

    renderComponent();

    const submitBtn = await screen.findByRole('button', { name: /Jelentkezes bekuldese/i });
    
    // Opcionális szöveg
    const textArea = screen.getByLabelText(/Uzenet/i);
    fireEvent.change(textArea, { target: { value: 'Szeretnék csatlakozni' } });
    
    fireEvent.click(submitBtn);

    await waitFor(() => {
      expect(societiesApi.join).toHaveBeenCalledWith(10, 'Szeretnék csatlakozni');
    });
    
    expect(await screen.findByText(/A jelentkezesed beert/i)).toBeInTheDocument();
  });

  it('does not allow Society accounts to join', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: true,
      user: { accountType: 'Society' },
    });

    (societiesApi.getById as any).mockResolvedValue({
      societyId: 10,
      name: 'Nagy Vadasz Klub',
      memberCount: 50,
      email: 'klub@vadasz.hu',
      createdAt: '2023-01-01T00:00:00Z',
    });

    renderComponent();

    expect(await screen.findByText(/Tarsasagi fiokkal nem lehet/i)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Jelentkezes bekuldese/i })).not.toBeInTheDocument();
  });

  it('displays login prompt for unauthenticated users', async () => {
    (useAuth as any).mockReturnValue({
      isAuthenticated: false,
      user: null,
    });

    (societiesApi.getById as any).mockResolvedValue({
      societyId: 10,
      name: 'Nagy Vadasz Klub',
      memberCount: 50,
      email: 'klub@vadasz.hu',
      createdAt: '2023-01-01T00:00:00Z',
    });

    renderComponent();

    expect(await screen.findByText(/A jelentkezeshez/i)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Jelentkezes bekuldese/i })).not.toBeInTheDocument();
  });
});
