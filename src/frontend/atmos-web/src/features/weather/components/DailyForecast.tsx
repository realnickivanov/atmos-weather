import type { DailyWeather } from '../api/weather.types'
import {
  formatDayName,
  formatPercent,
  formatShortDate,
  formatTemperature,
} from '../utils/formatters'
import { RainIcon, SunriseIcon, SunsetIcon, WindIcon } from './icons/UiIcons'
import { WeatherIcon } from './icons/WeatherIcon'

interface DailyForecastProps {
  daily: DailyWeather[]
  localDate: string
}

export function DailyForecast({ daily, localDate }: DailyForecastProps) {
  const allTemperatures = daily.flatMap((day) => [
    day.minimumTemperatureC,
    day.maximumTemperatureC,
  ])
  const overallMinimum = Math.min(...allTemperatures)
  const overallMaximum = Math.max(...allTemperatures)
  const overallRange = Math.max(overallMaximum - overallMinimum, 1)

  return (
    <section className="forecast-section daily-section glass-card" aria-labelledby="daily-title">
      <div className="section-heading">
        <div>
          <p className="section-kicker">Looking ahead</p>
          <h2 id="daily-title">3-day forecast</h2>
        </div>
      </div>

      <div className="daily-list">
        {daily.map((day, index) => {
          const left = ((day.minimumTemperatureC - overallMinimum) / overallRange) * 100
          const width = Math.max(
            ((day.maximumTemperatureC - day.minimumTemperatureC) / overallRange) * 100,
            10,
          )
          const constrainedWidth = Math.min(width, 100 - left)
          const label = index === 0 && day.date === localDate ? 'Today' : formatDayName(day.date)
          const gradientId = `daily-range-${index}`

          return (
            <article className="daily-row" key={day.date}>
              <div className="daily-row__date">
                <strong>{label}</strong>
                <time dateTime={day.date}>{formatShortDate(day.date)}</time>
              </div>

              <div className="daily-row__condition">
                <WeatherIcon code={day.conditionCode} isDay />
                <span>{day.conditionText}</span>
              </div>

              <div className="daily-row__details" aria-label="Daily weather details">
                <span title="Chance of rain">
                  <RainIcon /> {formatPercent(day.chanceOfRain)}
                </span>
                <span title="Maximum wind">
                  <WindIcon /> {Math.round(day.maximumWindKph)} km/h
                </span>
                <span title="Sunrise and sunset">
                  <SunriseIcon /> {day.sunrise}
                  <SunsetIcon /> {day.sunset}
                </span>
              </div>

              <div
                className="daily-range"
                aria-label={`Low ${Math.round(day.minimumTemperatureC)} degrees Celsius, high ${Math.round(day.maximumTemperatureC)} degrees Celsius`}
              >
                <span>{formatTemperature(day.minimumTemperatureC)}</span>
                <svg
                  className="daily-range__track"
                  viewBox="0 0 100 4"
                  preserveAspectRatio="none"
                  aria-hidden="true"
                >
                  <defs>
                    <linearGradient id={gradientId} x1="0" y1="0" x2="1" y2="0">
                      <stop offset="0" stopColor="#82d4ff" />
                      <stop offset="1" stopColor="#f8d57d" />
                    </linearGradient>
                  </defs>
                  <rect className="daily-range__base" x="0" y="1" width="100" height="2" rx="1" />
                  <rect
                    className="daily-range__value"
                    x={left}
                    y="1"
                    width={constrainedWidth}
                    height="2"
                    rx="1"
                    fill={`url(#${gradientId})`}
                  />
                </svg>
                <strong>{formatTemperature(day.maximumTemperatureC)}</strong>
              </div>
            </article>
          )
        })}
      </div>
    </section>
  )
}
