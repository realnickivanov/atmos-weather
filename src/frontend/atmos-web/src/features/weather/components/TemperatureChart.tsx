import { useId } from 'react'
import type { HourlyWeather } from '../api/weather.types'
import { formatTemperature } from '../utils/formatters'

interface TemperatureChartProps {
  hourly: HourlyWeather[]
}

export function TemperatureChart({ hourly }: TemperatureChartProps) {
  const gradientId = useId().replaceAll(':', '')

  if (hourly.length < 2) {
    return null
  }

  const width = hourly.length * 96 - 8
  const height = 170
  const pointInset = 44
  const paddingTop = 25
  const paddingBottom = 25
  const temperatures = hourly.map((item) => item.temperatureC)
  const minimum = Math.min(...temperatures)
  const maximum = Math.max(...temperatures)
  const range = Math.max(maximum - minimum, 1)
  const usableHeight = height - paddingTop - paddingBottom
  const usableWidth = width - pointInset * 2

  const points = temperatures.map((temperature, index) => ({
    x: pointInset + (index / (temperatures.length - 1)) * usableWidth,
    y: paddingTop + ((maximum - temperature) / range) * usableHeight,
  }))

  const linePath = points
    .map((point, index) => `${index === 0 ? 'M' : 'L'} ${point.x.toFixed(2)} ${point.y.toFixed(2)}`)
    .join(' ')
  const first = points[0]
  const last = points.at(-1)

  if (!first || !last) {
    return null
  }

  const areaPath = `${linePath} L ${last.x.toFixed(2)} ${height} L ${first.x.toFixed(2)} ${height} Z`

  return (
    <div className="temperature-chart">
      <div className="temperature-chart__labels" aria-hidden="true">
        <span>{formatTemperature(maximum)}</span>
        <span>{formatTemperature(minimum)}</span>
      </div>
      <svg
        viewBox={`0 0 ${width} ${height}`}
        preserveAspectRatio="none"
        role="img"
        aria-label={`Hourly temperature curve from ${formatTemperature(minimum)} to ${formatTemperature(maximum)}`}
      >
        <defs>
          <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0" stopColor="currentColor" stopOpacity="0.3" />
            <stop offset="1" stopColor="currentColor" stopOpacity="0" />
          </linearGradient>
        </defs>
        <path className="temperature-chart__area" d={areaPath} fill={`url(#${gradientId})`} />
        <path className="temperature-chart__line" d={linePath} />
      </svg>
    </div>
  )
}
