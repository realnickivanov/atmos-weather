import type { ReactNode } from 'react'
import type { WeatherResponse } from '../api/weather.types'
import { useLiveClock } from '../hooks/useLiveClock'
import {
  formatDecimal,
  formatLocationDate,
  formatLocationTime,
  formatPercent,
  formatTemperature,
} from '../utils/formatters'
import {
  DropletIcon,
  EyeIcon,
  GaugeIcon,
  LocationIcon,
  WindIcon,
} from './icons/UiIcons'
import { WeatherIcon } from './icons/WeatherIcon'

interface CurrentWeatherCardProps {
  weather: WeatherResponse
}

export function CurrentWeatherCard({ weather }: CurrentWeatherCardProps) {
  const { location, current, daily } = weather
  const today = daily[0]
  const localClock = useLiveClock(location.localTime)
  const localClockIso = localClock.toISOString()

  return (
    <section className="current-card glass-card" aria-labelledby="current-weather-title">
      <div className="current-card__main">
        <div className="current-card__copy">
          <p className="location-line">
            <LocationIcon />
            {location.name}, {location.country}
          </p>
          <p className="local-date">
            {formatLocationDate(localClockIso, location.timeZoneId)} ·{' '}
            {formatLocationTime(localClockIso, location.timeZoneId)}
          </p>
          <h1 id="current-weather-title" className="current-temperature">
            {formatTemperature(current.temperatureC)}
          </h1>
          <p className="current-condition">{current.conditionText}</p>
          <p className="current-summary">
            Feels like {formatTemperature(current.feelsLikeC)}
            {today && (
              <>
                <span aria-hidden="true"> · </span>
                <span>
                  High {formatTemperature(today.maximumTemperatureC)}, low{' '}
                  {formatTemperature(today.minimumTemperatureC)}
                </span>
              </>
            )}
          </p>
        </div>

        <div className="current-card__visual">
          <div className="weather-orbit" aria-hidden="true">
            <span className="weather-orbit__ring" />
            <span className="weather-orbit__glow" />
          </div>
          <WeatherIcon
            code={current.conditionCode}
            isDay={current.isDay}
            className="current-weather-icon"
          />
        </div>
      </div>

      <dl className="weather-metrics">
        <Metric
          icon={<DropletIcon />}
          label="Humidity"
          value={formatPercent(current.humidity)}
        />
        <Metric
          icon={<WindIcon />}
          label="Wind"
          value={`${Math.round(current.windKph)} km/h ${current.windDirection}`}
        />
        <Metric
          icon={<EyeIcon />}
          label="Visibility"
          value={formatDecimal(current.visibilityKm, ' km')}
        />
        <Metric
          icon={<GaugeIcon />}
          label="Pressure"
          value={`${Math.round(current.pressureMb)} mb`}
        />
      </dl>
    </section>
  )
}

interface MetricProps {
  icon: ReactNode
  label: string
  value: string
}

function Metric({ icon, label, value }: MetricProps) {
  return (
    <div className="weather-metric">
      <span className="weather-metric__icon">{icon}</span>
      <div>
        <dt>{label}</dt>
        <dd>{value}</dd>
      </div>
    </div>
  )
}
