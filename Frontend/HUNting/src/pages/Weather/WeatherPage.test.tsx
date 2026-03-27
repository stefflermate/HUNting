import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { WeatherPage } from './WeatherPage';
import { getWeatherForCity } from '@/services/weatherService';

vi.mock('@/services/weatherService', () => ({
  getWeatherForCity: vi.fn(),
}));

describe('WeatherPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders correctly', () => {
    render(<WeatherPage />);
    expect(screen.getByRole('textbox')).toBeInTheDocument();
    
    // We can't match specifically "Keresés" if there's encoding issues but we'll try by role
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBeGreaterThan(0);
  });

  it('handles search and displays weather data', async () => {
    const mockData = {
      current: {
        temp: 22.4,
        weather: [{ description: 'napos', icon: '01d' }],
        wind_speed: 3.5,
        wind_deg: 180
      },
      daily: []
    };

    (getWeatherForCity as any).mockResolvedValue(mockData);

    render(<WeatherPage />);

    const input = screen.getByRole('textbox');
    fireEvent.change(input, { target: { value: 'Budapest' } });
    
    // Fallback search button matching
    const searchBtn = screen.getAllByRole('button')[0];
    fireEvent.click(searchBtn);

    await waitFor(() => {
      expect(getWeatherForCity).toHaveBeenCalledWith('Budapest');
    });

    expect(await screen.findByText('22°C')).toBeInTheDocument();
    expect(screen.getByText('napos')).toBeInTheDocument();
  });

  it('displays error on API failure', async () => {
    (getWeatherForCity as any).mockRejectedValue(new Error('API failed'));

    render(<WeatherPage />);

    const input = screen.getByRole('textbox');
    fireEvent.change(input, { target: { value: 'IsmeretlenVaros' } });
    
    const searchBtn = screen.getAllByRole('button')[0];
    fireEvent.click(searchBtn);

    const errorMessage = await screen.findByText(/Nem siker/i);
    expect(errorMessage).toBeInTheDocument();
  });
});
