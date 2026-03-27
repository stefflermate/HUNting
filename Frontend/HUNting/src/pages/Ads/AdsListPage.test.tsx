import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, expect, test, vi, beforeEach } from 'vitest';
import { AdsListPage } from './AdsListPage';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as AuthContext from '@/context/AuthContext';
import { adsApi } from '@/services/adsApi';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/adsApi', () => ({
  adsApi: {
    categories: vi.fn(),
    list: vi.fn(),
  }
}));

describe('AdsListPage Component', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.resetAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    });

    (AuthContext.useAuth as any).mockReturnValue({
      isAuthenticated: true,
    });

    // Mock successful category fetch
    (adsApi.categories as any).mockResolvedValue([
      { categoryId: 1, name: 'Fegyverek' },
      { categoryId: 2, name: 'Ruházat' },
    ]);

    // Mock successful ads fetch
    (adsApi.list as any).mockResolvedValue({
      items: [
        { adId: 1, title: 'Vadászpuska eladó', price: 150000, categoryName: 'Fegyverek', createdAt: new Date().toISOString() },
      ],
      totalCount: 1,
      pageNumber: 1,
      pageSize: 9,
      totalPages: 1
    });
  });

  const renderComponent = () => render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AdsListPage />
      </BrowserRouter>
    </QueryClientProvider>
  );

  test('renders ads list and filters', async () => {
    renderComponent();

    // Check header
    expect(screen.getByText(/Piactér/i)).toBeInTheDocument();
    
    // Check create new ad button
    expect(screen.getByText(/Új hirdetés/i)).toBeInTheDocument();

    // Check filters
    expect(screen.getByPlaceholderText(/Keresés.../i)).toBeInTheDocument();
    
    // Wait for API calls to populate list
    await waitFor(() => {
      expect(screen.getByText('Vadászpuska eladó')).toBeInTheDocument();
      // Price is formatted probably, check part of the string
      expect(screen.getByText(/150.*000/)).toBeInTheDocument();
    });
  });

  test('calls API with updated filters on change', async () => {
    renderComponent();

    const searchInput = screen.getByPlaceholderText(/Keresés.../i);
    fireEvent.change(searchInput, { target: { value: 'teszt' } });

    // Assuming min price has placeholder Min. ár
    const minPriceInput = screen.getByPlaceholderText(/Mettol/i);
    fireEvent.change(minPriceInput, { target: { value: '1000' } });

    await waitFor(() => {
      expect(adsApi.list).toHaveBeenCalledWith(
        expect.objectContaining({
          search: 'teszt',
          minPrice: 1000,
        })
      );
    });
  });
});
