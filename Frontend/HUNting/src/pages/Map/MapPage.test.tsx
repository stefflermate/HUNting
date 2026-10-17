import { render, screen, waitFor } from '@testing-library/react';
import { describe, expect, test, vi, beforeEach } from 'vitest';
import { MapPage } from './MapPage';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as AuthContext from '@/context/AuthContext';
import { mapApi } from '@/services/mapApi';

// Need to mock Google Maps components deeply for testing
vi.mock('@react-google-maps/api', () => ({
  GoogleMap: ({ children }: any) => <div data-testid="google-map">{children}</div>,
  MarkerF: () => <div data-testid="marker-f" />,
  PolygonF: () => <div data-testid="polygon-f" />,
  InfoWindowF: ({ children }: any) => <div data-testid="info-window-f">{children}</div>,
  DrawingManagerF: () => <div data-testid="drawing-manager-f" />,
  useLoadScript: () => ({ isLoaded: true, loadError: null }), // Mock it as successfully loaded
}));

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/mapApi', () => ({
  mapApi: {
    getMarkers: vi.fn(),
    listMarkers: vi.fn(),
    listPolygons: vi.fn(),
    getPolygons: vi.fn(),
    getIconTypes: vi.fn(),
    getConfig: vi.fn(), // need to mock the config
  }
}));

// We need to mock societyApi as well
vi.mock('@/services/societiesApi', () => ({
  societiesApi: {
    my: vi.fn().mockResolvedValue([]),
  }
}));

describe('MapPage Component', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.resetAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });

    (AuthContext.useAuth as any).mockReturnValue({
      user: { id: "1" },
      isAuthenticated: true,
    });

    (mapApi.getConfig as any).mockResolvedValue({
      iconTypes: [{ iconTypeId: 1, name: 'Leles', url: '/icon.png' }],
      visibilities: [{ visibilityCode: 'PUBLIC' }]
    });

    (mapApi.listMarkers as any).mockResolvedValue([
      { markerId: 1, latitude: 47.5, longitude: 19.0, note: 'Teszt marker' }
    ]);
    (mapApi.listPolygons as any).mockResolvedValue([
      { polygonId: 1, name: 'Teszt Terulet', points: [] }
    ]);
  });

  test('renders map correctly', async () => {
    // Prevent environment variable missing errors
    import.meta.env.VITE_GOOGLE_MAPS_API_KEY = "dummy-key";
    
    render(
      <QueryClientProvider client={queryClient}>
         <MapPage />
      </QueryClientProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('google-map')).toBeInTheDocument();
    });
  });
});
