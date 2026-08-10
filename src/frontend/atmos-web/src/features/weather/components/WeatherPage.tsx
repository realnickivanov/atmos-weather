import { getWeatherMood } from '../utils/condition'
import { formatLocationTime, getDateKey } from '../utils/formatters'
import { useWeather } from '../hooks/useWeather'
import { BrandHeader } from './BrandHeader'
import { CurrentWeatherCard } from './CurrentWeatherCard'
import { DailyForecast } from './DailyForecast'
import { ErrorWeather } from './ErrorWeather'
import { HourlyForecast } from './HourlyForecast'
import { LoadingWeather } from './LoadingWeather'

export function WeatherPage() {
  const query = useWeather()

  if (query.isPending) {
    return <LoadingWeather />
  }

  if (query.isError) {
    return (
      <ErrorWeather
        error={query.error}
        isRetrying={query.isFetching}
        onRetry={() => void query.refetch()}
      />
    )
  }

  const weather = query.data
  const mood = getWeatherMood(weather.current.conditionCode, weather.current.isDay)
  const localDate = getDateKey(weather.location.localTime)

  return (
    <div className="weather-shell" data-mood={mood}>
      <a className="skip-link" href="#forecast-content">
        Skip to forecast
      </a>
      <main className="weather-app" id="forecast-content">
        <BrandHeader
          isRefreshing={query.isFetching}
          onRefresh={() => void query.refetch()}
        />
        <div className="sr-only" aria-live="polite">
          {query.isFetching ? 'Refreshing weather forecast' : 'Weather forecast is up to date'}
        </div>

        <CurrentWeatherCard weather={weather} />
        <HourlyForecast
          hourly={weather.hourly}
          localDate={localDate}
          timeZone={weather.location.timeZoneId}
        />
        <DailyForecast daily={weather.daily} localDate={localDate} />

        <footer className="app-footer">
          <span>
            Updated {formatLocationTime(weather.current.observedAt, weather.location.timeZoneId)}
          </span>
          <span aria-hidden="true">·</span>
          <span>Forecast for Moscow coordinates</span>
        </footer>
      </main>
    </div>
  )
}
