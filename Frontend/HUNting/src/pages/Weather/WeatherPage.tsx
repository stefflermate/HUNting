import React, { useState } from 'react';
import { getWeatherForCity, type WeatherData } from '@/services/weatherService';
import { Button } from '@/components/ui/button';
import { ArrowUp } from 'lucide-react';

export const WeatherPage: React.FC = () => {
    const [city, setCity] = useState('');
    const [weather, setWeather] = useState<WeatherData | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSearch = async () => {
        if (!city.trim()) return;

        setLoading(true);
        setError(null);
        try {
            const data = await getWeatherForCity(city);
            setWeather(data);
        } catch (err) {
            setError('Nem sikerült lekérni az időjárást. Ellenőrizd a város nevét.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="container mx-auto p-4 text-foreground">
            <h1 className="text-2xl font-bold mb-4 font-display text-accent">Időjárás</h1>
            
            <div className="flex gap-2 mb-6">
                <input
                    type="text"
                    value={city}
                    onChange={(e) => setCity(e.target.value)}
                    placeholder="Írj be egy várost..."
                    className="border border-muted bg-background-soft p-2 rounded flex-grow text-foreground placeholder:text-foreground-muted focus:outline-none focus:border-accent"
                    onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                />
                <Button onClick={handleSearch} disabled={loading} className="bg-accent text-background hover:bg-accent-soft">
                    {loading ? 'Keresés...' : 'Keresés'}
                </Button>
            </div>

            {error && <div className="text-red-500 mb-4">{error}</div>}

            {weather && (
                <div className="grid gap-6">
                    {/* Current Weather */}
                    <div className="bg-card p-6 rounded-lg shadow-md border border-muted">
                        <h2 className="text-xl font-semibold mb-4 text-accent-soft">Jelenlegi időjárás</h2>
                        <div className="flex items-center gap-4">
                            <img 
                                src={`http://openweathermap.org/img/wn/${weather.current.weather[0].icon}@2x.png`} 
                                alt={weather.current.weather[0].description}
                                className="w-20 h-20"
                            />
                            <div>
                                <p className="text-3xl font-bold text-accent">{Math.round(weather.current.temp)}°C</p>
                                <p className="text-foreground-muted capitalize">{weather.current.weather[0].description}</p>
                                <p className="text-sm text-foreground-muted mt-2">Hőérzet: {Math.round(weather.current.feels_like)}°C</p>
                                <p className="text-sm text-foreground-muted">Páratartalom: {weather.current.humidity}%</p>
                                <div className="flex items-center gap-2 mt-1">
                                    <p className="text-sm text-foreground-muted">Szél: {weather.current.wind_speed} m/s</p>
                                    <ArrowUp 
                                        className="w-4 h-4 text-foreground-muted" 
                                        style={{ transform: `rotate(${weather.current.wind_deg}deg)` }} 
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};
