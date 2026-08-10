import type { HourlyWeather, WeatherResponse } from '../features/weather/api/weather.types'

const todayHours = createHours('2026-08-10', 17, 7)
const tomorrowHours = createHours('2026-08-11', 0, 24)

export const weatherFixture: WeatherResponse = {
  location: {
    name: 'Moscow',
    region: 'Moscow City',
    country: 'Russia',
    timeZoneId: 'Europe/Moscow',
    latitude: 55.7558,
    longitude: 37.6173,
    localTime: '2026-08-10T16:15:00+03:00',
  },
  current: {
    observedAt: '2026-08-10T16:00:00+03:00',
    temperatureC: 24,
    feelsLikeC: 25,
    conditionCode: 1003,
    conditionText: 'Partly cloudy',
    isDay: true,
    windKph: 12,
    windDirection: 'WSW',
    gustKph: 18,
    humidity: 58,
    pressureMb: 1013,
    precipitationMm: 0,
    visibilityKm: 10,
    uvIndex: 4,
  },
  hourly: [...todayHours, ...tomorrowHours],
  daily: [
    {
      date: '2026-08-10',
      minimumTemperatureC: 16,
      maximumTemperatureC: 26,
      averageTemperatureC: 21,
      conditionCode: 1003,
      conditionText: 'Partly cloudy',
      chanceOfRain: 20,
      maximumWindKph: 20,
      averageHumidity: 62,
      uvIndex: 5,
      sunrise: '05:10 AM',
      sunset: '08:25 PM',
    },
    {
      date: '2026-08-11',
      minimumTemperatureC: 15,
      maximumTemperatureC: 23,
      averageTemperatureC: 19,
      conditionCode: 1063,
      conditionText: 'Patchy rain possible',
      chanceOfRain: 65,
      maximumWindKph: 24,
      averageHumidity: 70,
      uvIndex: 3,
      sunrise: '05:12 AM',
      sunset: '08:23 PM',
    },
    {
      date: '2026-08-12',
      minimumTemperatureC: 14,
      maximumTemperatureC: 25,
      averageTemperatureC: 20,
      conditionCode: 1000,
      conditionText: 'Sunny',
      chanceOfRain: 10,
      maximumWindKph: 17,
      averageHumidity: 60,
      uvIndex: 5,
      sunrise: '05:14 AM',
      sunset: '08:20 PM',
    },
  ],
}

function createHours(date: string, startHour: number, count: number): HourlyWeather[] {
  return Array.from({ length: count }, (_, index) => {
    const hour = startHour + index
    const isDay = hour >= 6 && hour < 20
    const isRainy = date === '2026-08-11' && hour >= 12 && hour <= 18
    const conditionCode = isRainy ? 1063 : isDay ? 1003 : 1000
    const conditionText = isRainy ? 'Patchy rain possible' : isDay ? 'Partly cloudy' : 'Clear'
    const temperatureC = Math.round(17 + Math.sin((hour / 24) * Math.PI) * 7)

    return {
      time: `${date}T${String(hour).padStart(2, '0')}:00:00+03:00`,
      temperatureC,
      feelsLikeC: temperatureC + (isDay ? 1 : 0),
      conditionCode,
      conditionText,
      isDay,
      chanceOfRain: isRainy ? 65 : isDay ? 10 : 5,
      humidity: isDay ? 60 : 72,
      windKph: 8 + (hour % 5),
    }
  })
}
