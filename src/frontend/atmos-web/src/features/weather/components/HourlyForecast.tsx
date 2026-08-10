import type { HourlyWeather } from '../api/weather.types'
import {
  formatHour,
  formatPercent,
  formatTemperature,
  getDateKey,
} from '../utils/formatters'
import { ArrowIcon, RainIcon } from './icons/UiIcons'
import { WeatherIcon } from './icons/WeatherIcon'
import { TemperatureChart } from './TemperatureChart'

interface HourlyForecastProps {
  hourly: HourlyWeather[]
  localDate: string
  timeZone: string
}

export function HourlyForecast({ hourly, localDate, timeZone }: HourlyForecastProps) {
  return (
    <section className="forecast-section glass-card" aria-labelledby="hourly-title">
      <div className="section-heading">
        <div>
          <p className="section-kicker">Next up</p>
          <h2 id="hourly-title">Hourly forecast</h2>
        </div>
        <span className="scroll-hint" aria-hidden="true">
          Scroll to explore <ArrowIcon />
        </span>
      </div>

      <div
        className="hourly-scroller"
        role="region"
        tabIndex={0}
        aria-label="Scrollable hourly forecast"
      >
        <div className="hourly-timeline">
          <TemperatureChart hourly={hourly} />

          <div className="hourly-track">
            {hourly.map((hour, index) => {
              const dateKey = getDateKey(hour.time)
              const previousDate = index > 0 ? getDateKey(hourly[index - 1]?.time ?? '') : ''
              const startsDay = index === 0 || dateKey !== previousDate
              const dayLabel = dateKey === localDate ? 'Today' : 'Tomorrow'

              return (
                <article
                  className="hour-card"
                  key={hour.time}
                  aria-label={`${dayLabel}, ${formatHour(hour.time, timeZone)}, ${Math.round(hour.temperatureC)} degrees Celsius, ${hour.conditionText}`}
                >
                  <span
                    className={`hour-card__day${startsDay ? '' : ' is-placeholder'}`}
                    aria-hidden={!startsDay}
                  >
                    {startsDay ? dayLabel : 'Day'}
                  </span>
                  <time dateTime={hour.time}>{formatHour(hour.time, timeZone)}</time>
                  <WeatherIcon code={hour.conditionCode} isDay={hour.isDay} />
                  <strong>{formatTemperature(hour.temperatureC)}</strong>
                  <span className="hour-card__rain">
                    <RainIcon />
                    {formatPercent(hour.chanceOfRain)}
                  </span>
                </article>
              )
            })}
          </div>
        </div>
      </div>
    </section>
  )
}
