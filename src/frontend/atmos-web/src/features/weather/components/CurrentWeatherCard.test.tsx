import { act, cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { weatherFixture } from '../../../test/weatherFixture'
import { CurrentWeatherCard } from './CurrentWeatherCard'

afterEach(() => {
  cleanup()
  vi.useRealTimers()
})

describe('CurrentWeatherCard', () => {
  it('keeps a live Moscow clock anchored to server time instead of the client clock', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2040-01-01T00:00:00Z'))

    const weather = {
      ...weatherFixture,
      location: {
        ...weatherFixture.location,
        localTime: '2026-08-10T21:36:30+03:00',
      },
    }

    render(<CurrentWeatherCard weather={weather} />)

    expect(screen.getByText(/9:36 PM/)).toBeInTheDocument()

    act(() => {
      vi.advanceTimersByTime(31_000)
    })

    expect(screen.getByText(/9:37 PM/)).toBeInTheDocument()
  })
})
