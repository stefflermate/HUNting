import axios from 'axios';

const API_KEY = import.meta.env.VITE_OPEN_WEATHER_API;
const WEATHER_URL = 'https://api.openweathermap.org/data/2.5/weather';

export interface WeatherData {
    current: {
        dt: number;
        temp: number;
        feels_like: number;
        humidity: number;
        weather: {
            main: string;
            description: string;
            icon: string;
        }[];
        wind_speed: number;
        wind_deg: number;
    };
}

export const getWeatherForCity = async (city: string): Promise<WeatherData | null> => {
    if (!API_KEY) {
        console.error("OpenWeather API key is missing!");
        return null;
    }

    try {
        // 1. Get current weather AND coordinates by city name directly
        // This avoids using the separate Geo API which might have different activation times or issues
        const currentResponse = await axios.get(WEATHER_URL, {
            params: {
                q: city,
                units: 'metric',
                lang: 'hu',
                appid: API_KEY
            }
        });

        return {
            current: {
                dt: currentResponse.data.dt,
                temp: currentResponse.data.main.temp,
                feels_like: currentResponse.data.main.feels_like,
                humidity: currentResponse.data.main.humidity,
                weather: currentResponse.data.weather,
                wind_speed: currentResponse.data.wind.speed,
                wind_deg: currentResponse.data.wind.deg
            }
        };

    } catch (error) {
        console.error("Error fetching weather data:", error);
        throw error;
    }
};
