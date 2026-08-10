export interface WeatherResponse {
  location: WeatherLocation
  current: CurrentWeather
  hourly: HourlyWeather[]
  daily: DailyWeather[]
}

export interface WeatherLocation {
  name: string
  region: string
  country: string
  timeZoneId: string
  latitude: number
  longitude: number
  localTime: string
}

export interface CurrentWeather {
  observedAt: string
  temperatureC: number
  feelsLikeC: number
  conditionCode: number
  conditionText: string
  isDay: boolean
  windKph: number
  windDirection: string
  gustKph: number
  humidity: number
  pressureMb: number
  precipitationMm: number
  visibilityKm: number
  uvIndex: number
}

export interface HourlyWeather {
  time: string
  temperatureC: number
  feelsLikeC: number
  conditionCode: number
  conditionText: string
  isDay: boolean
  chanceOfRain: number
  humidity: number
  windKph: number
}

export interface DailyWeather {
  date: string
  minimumTemperatureC: number
  maximumTemperatureC: number
  averageTemperatureC: number
  conditionCode: number
  conditionText: string
  chanceOfRain: number
  maximumWindKph: number
  averageHumidity: number
  uvIndex: number
  sunrise: string
  sunset: string
}

export interface ApiProblem {
  title?: string
  detail?: string
  status?: number
  traceId?: string
}
