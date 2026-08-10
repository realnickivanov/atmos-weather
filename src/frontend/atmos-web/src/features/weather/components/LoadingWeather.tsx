import { BrandHeader } from './BrandHeader'

export function LoadingWeather() {
  return (
    <div className="weather-shell" data-mood="clouds">
      <a className="skip-link" href="#forecast-content">
        Skip to forecast
      </a>
      <main className="weather-app" id="forecast-content">
        <BrandHeader />
        <div className="loading-announcement" role="status" aria-live="polite">
          <span className="sr-only">Loading weather forecast</span>
        </div>

        <section className="current-card glass-card skeleton-card" aria-hidden="true">
          <div className="skeleton-current">
            <div>
              <span className="skeleton skeleton--line skeleton--location" />
              <span className="skeleton skeleton--line skeleton--date" />
              <span className="skeleton skeleton--temperature" />
              <span className="skeleton skeleton--line skeleton--condition" />
            </div>
            <span className="skeleton skeleton--icon" />
          </div>
          <div className="skeleton-metrics">
            {Array.from({ length: 4 }, (_, index) => (
              <span className="skeleton skeleton--metric" key={index} />
            ))}
          </div>
        </section>

        <section className="forecast-section glass-card skeleton-card" aria-hidden="true">
          <span className="skeleton skeleton--line skeleton--heading" />
          <span className="skeleton skeleton--chart" />
          <div className="skeleton-hours">
            {Array.from({ length: 8 }, (_, index) => (
              <span className="skeleton skeleton--hour" key={index} />
            ))}
          </div>
        </section>

        <section className="forecast-section glass-card skeleton-card" aria-hidden="true">
          <span className="skeleton skeleton--line skeleton--heading" />
          {Array.from({ length: 3 }, (_, index) => (
            <span className="skeleton skeleton--day" key={index} />
          ))}
        </section>
      </main>
    </div>
  )
}
