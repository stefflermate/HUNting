import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, expect, test, vi, beforeEach } from 'vitest';
import { AdDetailPage } from './AdDetailPage';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as AuthContext from '@/context/AuthContext';
import { adsApi } from '@/services/adsApi';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/adsApi', () => ({
  adsApi: {
    getById: vi.fn(),
    remove: vi.fn(),
  }
}));

describe('AdDetailPage Component', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.resetAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    });

    (AuthContext.useAuth as any).mockReturnValue({
      user: { id: "1", roles: ["User"] },
      isAuthenticated: true,
    });

    (adsApi.getById as any).mockResolvedValue({
      adId: 10,
      title: 'Eladó távcső test',
      description: 'Nagyon jó állapotú távcső eladó kihasználatlanság miatt',
      price: 50000,
      locationText: 'Budapest',
      statusCode: 'Active',
      ownerUserId: "1", // Same as user to show delete button
      category: { name: 'Kiegészítő' },
      createdAt: new Date().toISOString(),
      images: []
    });

    (adsApi.remove as any).mockResolvedValue({});
  });

  const renderComponent = (adId: string) => render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[/ads/ + adId]}>
        <Routes>
          <Route path="/ads/:adId" element={<AdDetailPage />} />
          <Route path="/ads" element={<div>Ads List Page</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>
  );

  test('renders ad details successfully', async () => {
    renderComponent('10');

    await waitFor(() => {
      expect(screen.getByText('Eladó távcső test')).toBeInTheDocument();
      expect(screen.getByText('Nagyon jó állapotú távcső eladó kihasználatlanság miatt')).toBeInTheDocument();
      expect(screen.getByText('Budapest')).toBeInTheDocument();
      expect(screen.getByText('Kiegészítő')).toBeInTheDocument();
    });
  });

  test('shows delete button for ad owner and handles deletion', async () => {
    // We cannot spy on window.confirm directly in happy-dom if it is not defined yet, we can mock it on the global window object.
    window.confirm = vi.fn().mockReturnValue(true);
    
    renderComponent('10');

    // Wait for data load
    await waitFor(() => {
      expect(screen.getByText('Eladó távcső test')).toBeInTheDocument();
    });

    // The owner's delete button should exist
    const deleteButton = screen.getByRole('button', { name: /Torles/i });
    expect(deleteButton).toBeInTheDocument();

    fireEvent.click(deleteButton);

    await waitFor(() => {
      expect(adsApi.remove).toHaveBeenCalledWith(10);
      expect(screen.getByText('Ads List Page')).toBeInTheDocument(); // redirected to /ads
    });
    
  });
  
  test('does not show edit/delete buttons for non-owner', async () => {
    (AuthContext.useAuth as any).mockReturnValue({
      user: { id: "2", roles: ["User"] }, // Different user
      isAuthenticated: true,
    });
    
    renderComponent('10');

    await waitFor(() => {
      expect(screen.getByText('Eladó távcső test')).toBeInTheDocument();
    });

    expect(screen.queryByRole('button', { name: /Torles/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /Szerkesztes/i })).not.toBeInTheDocument();
  });
});
