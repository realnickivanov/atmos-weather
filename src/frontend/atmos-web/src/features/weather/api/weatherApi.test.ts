import { afterEach, describe, expect, it, vi } from 'vitest'
import { weatherFixture } from '../../../test/weatherFixture'
import { fetchWeather } from './weatherApi'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('fetchWeather', () => {
  it('accepts the complete backend contract', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(weatherFixture)))

    const result = await fetchWeather(new AbortController().signal)

    expect(result).toEqual(weatherFixture)
  })

  it('rejects a successful response with an incomplete hourly forecast', async () => {
    const invalidPayload = {
      ...weatherFixture,
      hourly: weatherFixture.hourly.slice(0, 23),
    }
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(invalidPayload)))

    await expect(fetchWeather(new AbortController().signal)).rejects.toMatchObject({
      name: 'WeatherRequestError',
      message: 'The weather service returned an invalid response.',
    })
  })

  it('rejects a daily average outside the minimum and maximum range', async () => {
    const invalidPayload = {
      ...weatherFixture,
      daily: weatherFixture.daily.map((day, index) =>
        index === 0 ? { ...day, averageTemperatureC: day.maximumTemperatureC + 1 } : day,
      ),
    }
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(invalidPayload)))

    await expect(fetchWeather(new AbortController().signal)).rejects.toMatchObject({
      name: 'WeatherRequestError',
      message: 'The weather service returned an invalid response.',
    })
  })

  it('rejects a calendar date that only looks like ISO syntax', async () => {
    const invalidPayload = {
      ...weatherFixture,
      daily: weatherFixture.daily.map((day, index) =>
        index === 0 ? { ...day, date: '2026-02-30' } : day,
      ),
    }
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(invalidPayload)))

    await expect(fetchWeather(new AbortController().signal)).rejects.toMatchObject({
      name: 'WeatherRequestError',
      message: 'The weather service returned an invalid response.',
    })
  })

  it('preserves safe Problem Details information', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        jsonResponse(
          {
            title: 'Weather service unavailable',
            detail: 'The latest weather information could not be retrieved.',
            status: 502,
            traceId: 'trace-123',
          },
          502,
        ),
      ),
    )

    await expect(fetchWeather(new AbortController().signal)).rejects.toMatchObject({
      name: 'WeatherRequestError',
      message: 'The latest weather information could not be retrieved.',
      status: 502,
      traceId: 'trace-123',
    })
  })
})

function jsonResponse(payload: unknown, status = 200): Response {
  return new Response(JSON.stringify(payload), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}
