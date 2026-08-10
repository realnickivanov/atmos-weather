import { useQuery } from '@tanstack/react-query'
import { fetchWeather, WeatherRequestError } from '../api/weatherApi'

export const weatherQueryKey = ['weather', 'moscow'] as const
const refreshInterval = 5 * 60 * 1_000

export function useWeather() {
  return useQuery({
    queryKey: weatherQueryKey,
    queryFn: ({ signal }) => fetchWeather(signal),
    staleTime: refreshInterval,
    refetchInterval: refreshInterval,
    refetchIntervalInBackground: false,
    refetchOnWindowFocus: true,
    retry: (failureCount, error) => {
      if (failureCount >= 1) {
        return false
      }

      return !(error instanceof WeatherRequestError && error.status !== undefined && error.status < 500)
    },
    retryDelay: 150,
  })
}
