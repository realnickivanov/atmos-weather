import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { weatherFixture } from '../../../test/weatherFixture'
import { WeatherPage } from './WeatherPage'

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

describe('WeatherPage', () => {
  it('shows a loading state while the request is pending', () => {
    vi.stubGlobal('fetch', vi.fn(() => new Promise<Response>(() => undefined)))

    renderPage()

    expect(screen.getByRole('status')).toHaveTextContent('Loading weather forecast')
  })

  it('renders current, hourly, and three-day weather', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(weatherFixture)))

    renderPage()

    expect(await screen.findByRole('heading', { name: '24°' })).toBeInTheDocument()
    expect(screen.getAllByText('Partly cloudy')).not.toHaveLength(0)
    expect(screen.getByRole('heading', { name: 'Hourly forecast' })).toBeInTheDocument()
    const hourlyScroller = screen.getByRole('region', { name: 'Scrollable hourly forecast' })
    const temperatureCurve = screen.getByRole('img', { name: /Hourly temperature curve/ })
    expect(hourlyScroller).toContainElement(temperatureCurve)
    expect(screen.getByRole('heading', { name: '3-day forecast' })).toBeInTheDocument()
    expect(screen.getByText('Tomorrow')).toBeInTheDocument()
  })

  it('shows an error and retries without reloading the page', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ detail: 'Upstream unavailable' }, 502))
      .mockResolvedValueOnce(jsonResponse({ detail: 'Upstream unavailable' }, 502))
      .mockResolvedValueOnce(jsonResponse(weatherFixture))
    vi.stubGlobal('fetch', fetchMock)

    renderPage()

    expect(await screen.findByRole('alert')).toHaveTextContent('Upstream unavailable')
    fireEvent.click(screen.getByRole('button', { name: 'Try again' }))

    expect(await screen.findByRole('heading', { name: '24°' })).toBeInTheDocument()
    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(3))
  })
})

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { gcTime: Infinity },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <WeatherPage />
    </QueryClientProvider>,
  )
}

function jsonResponse(payload: unknown, status = 200): Response {
  return new Response(JSON.stringify(payload), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}
