import type {
  ApiProblem,
  CurrentWeather,
  DailyWeather,
  HourlyWeather,
  WeatherLocation,
  WeatherResponse,
} from './weather.types'

export class WeatherRequestError extends Error {
  public constructor(
    message: string,
    public readonly status?: number,
    public readonly traceId?: string,
  ) {
    super(message)
    this.name = 'WeatherRequestError'
  }
}

export async function fetchWeather(signal: AbortSignal): Promise<WeatherResponse> {
  let response: Response

  try {
    response = await fetch('/api/weather', {
      method: 'GET',
      headers: { Accept: 'application/json' },
      signal,
    })
  } catch (error) {
    if (signal.aborted || isAbortError(error)) {
      throw createAbortError()
    }

    throw new WeatherRequestError('The weather service could not be reached.')
  }

  if (!response.ok) {
    const problem = await readProblem(response)
    throw new WeatherRequestError(
      problem.detail ?? problem.title ?? 'The latest forecast could not be loaded.',
      response.status,
      problem.traceId,
    )
  }

  let payload: unknown
  try {
    payload = await response.json()
  } catch (error) {
    if (signal.aborted || isAbortError(error)) {
      throw createAbortError()
    }

    throw new WeatherRequestError('The weather service returned invalid JSON.')
  }

  if (!isWeatherResponse(payload)) {
    throw new WeatherRequestError('The weather service returned an invalid response.')
  }

  return payload
}

async function readProblem(response: Response): Promise<ApiProblem> {
  try {
    const payload: unknown = await response.json()
    if (!isObject(payload)) {
      return {}
    }

    const problem: ApiProblem = {}
    const title = optionalString(payload.title)
    const detail = optionalString(payload.detail)
    const traceId = optionalString(payload.traceId)

    if (title !== undefined) problem.title = title
    if (detail !== undefined) problem.detail = detail
    if (isFiniteNumber(payload.status)) problem.status = payload.status
    if (traceId !== undefined) problem.traceId = traceId

    return problem
  } catch {
    return {}
  }
}

function isWeatherResponse(value: unknown): value is WeatherResponse {
  if (!isObject(value)) {
    return false
  }

  if (
    !isWeatherLocation(value.location) ||
    !isCurrentWeather(value.current) ||
    !Array.isArray(value.hourly) ||
    !Array.isArray(value.daily)
  ) {
    return false
  }

  if (value.hourly.length < 24 || value.hourly.length > 48 || value.daily.length !== 3) {
    return false
  }

  if (!value.hourly.every(isHourlyWeather) || !value.daily.every(isDailyWeather)) {
    return false
  }

  return (
    isStrictlyAscending(value.hourly.map((hour) => Date.parse(hour.time))) &&
    isStrictlyAscending(value.daily.map((day) => Date.parse(`${day.date}T00:00:00Z`)))
  )
}

function isWeatherLocation(value: unknown): value is WeatherLocation {
  return (
    isObject(value) &&
    isNonEmptyString(value.name) &&
    typeof value.region === 'string' &&
    isNonEmptyString(value.country) &&
    isSupportedTimeZone(value.timeZoneId) &&
    isFiniteNumber(value.latitude) &&
    value.latitude >= -90 &&
    value.latitude <= 90 &&
    isFiniteNumber(value.longitude) &&
    value.longitude >= -180 &&
    value.longitude <= 180 &&
    isIsoDateTime(value.localTime)
  )
}

function isCurrentWeather(value: unknown): value is CurrentWeather {
  return (
    isObject(value) &&
    isIsoDateTime(value.observedAt) &&
    isFiniteNumber(value.temperatureC) &&
    isFiniteNumber(value.feelsLikeC) &&
    isPositiveInteger(value.conditionCode) &&
    isNonEmptyString(value.conditionText) &&
    typeof value.isDay === 'boolean' &&
    isNonNegativeNumber(value.windKph) &&
    isNonEmptyString(value.windDirection) &&
    isNonNegativeNumber(value.gustKph) &&
    isPercentage(value.humidity) &&
    isPositiveNumber(value.pressureMb) &&
    isNonNegativeNumber(value.precipitationMm) &&
    isNonNegativeNumber(value.visibilityKm) &&
    isNonNegativeNumber(value.uvIndex)
  )
}

function isHourlyWeather(value: unknown): value is HourlyWeather {
  return (
    isObject(value) &&
    isIsoDateTime(value.time) &&
    isFiniteNumber(value.temperatureC) &&
    isFiniteNumber(value.feelsLikeC) &&
    isPositiveInteger(value.conditionCode) &&
    isNonEmptyString(value.conditionText) &&
    typeof value.isDay === 'boolean' &&
    isPercentage(value.chanceOfRain) &&
    isPercentage(value.humidity) &&
    isNonNegativeNumber(value.windKph)
  )
}

function isDailyWeather(value: unknown): value is DailyWeather {
  return (
    isObject(value) &&
    isDateOnly(value.date) &&
    isFiniteNumber(value.minimumTemperatureC) &&
    isFiniteNumber(value.maximumTemperatureC) &&
    value.minimumTemperatureC <= value.maximumTemperatureC &&
    isFiniteNumber(value.averageTemperatureC) &&
    value.averageTemperatureC >= value.minimumTemperatureC &&
    value.averageTemperatureC <= value.maximumTemperatureC &&
    isPositiveInteger(value.conditionCode) &&
    isNonEmptyString(value.conditionText) &&
    isPercentage(value.chanceOfRain) &&
    isNonNegativeNumber(value.maximumWindKph) &&
    isPercentage(value.averageHumidity) &&
    isNonNegativeNumber(value.uvIndex) &&
    isNonEmptyString(value.sunrise) &&
    isNonEmptyString(value.sunset)
  )
}

function isStrictlyAscending(values: number[]): boolean {
  return values.every(
    (value, index) => Number.isFinite(value) && (index === 0 || value > values[index - 1]!),
  )
}

function isSupportedTimeZone(value: unknown): value is string {
  if (!isNonEmptyString(value)) {
    return false
  }

  try {
    new Intl.DateTimeFormat('en-US', { timeZone: value }).format()
    return true
  } catch {
    return false
  }
}

function isIsoDateTime(value: unknown): value is string {
  return isNonEmptyString(value) && Number.isFinite(Date.parse(value))
}

function isDateOnly(value: unknown): value is string {
  if (typeof value !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return false
  }

  const timestamp = Date.parse(`${value}T00:00:00Z`)
  return Number.isFinite(timestamp) && new Date(timestamp).toISOString().slice(0, 10) === value
}

function isPercentage(value: unknown): value is number {
  return typeof value === 'number' && Number.isInteger(value) && value >= 0 && value <= 100
}

function isPositiveInteger(value: unknown): value is number {
  return typeof value === 'number' && Number.isInteger(value) && value > 0
}

function isNonNegativeNumber(value: unknown): value is number {
  return isFiniteNumber(value) && value >= 0
}

function isPositiveNumber(value: unknown): value is number {
  return isFiniteNumber(value) && value > 0
}

function isFiniteNumber(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value)
}

function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0
}

function optionalString(value: unknown): string | undefined {
  return typeof value === 'string' ? value : undefined
}

function isObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'AbortError'
}

function createAbortError(): DOMException {
  return new DOMException('The request was aborted.', 'AbortError')
}
