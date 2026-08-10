import { WeatherRequestError } from '../api/weatherApi'
import { BrandHeader } from './BrandHeader'
import { RefreshIcon } from './icons/UiIcons'

interface ErrorWeatherProps {
  error: Error
  isRetrying: boolean
  onRetry: () => void
}

export function ErrorWeather({ error, isRetrying, onRetry }: ErrorWeatherProps) {
  const traceId = error instanceof WeatherRequestError ? error.traceId : undefined

  return (
    <div className="weather-shell" data-mood="storm">
      <main className="weather-app error-layout">
        <BrandHeader />
        <section className="error-card glass-card" role="alert" aria-labelledby="error-title">
          <div className="error-illustration" aria-hidden="true">
            <span className="error-illustration__cloud" />
            <span className="error-illustration__bolt" />
            <span className="error-illustration__ring" />
          </div>
          <p className="section-kicker">Forecast interrupted</p>
          <h1 id="error-title">The sky went quiet.</h1>
          <p>
            {error.message || 'We could not retrieve the latest weather for Moscow.'}
          </p>
          <button className="primary-button" type="button" onClick={onRetry} disabled={isRetrying}>
            <RefreshIcon className={isRetrying ? 'is-spinning' : ''} />
            {isRetrying ? 'Trying again…' : 'Try again'}
          </button>
          {traceId && <small>Reference: {traceId}</small>}
        </section>
      </main>
    </div>
  )
}
