import { RefreshIcon } from './icons/UiIcons'

interface BrandHeaderProps {
  isRefreshing?: boolean
  onRefresh?: () => void
}

export function BrandHeader({ isRefreshing = false, onRefresh }: BrandHeaderProps) {
  return (
    <header className="brand-header">
      <div className="brand-lockup" aria-label="Atmos weather">
        <span className="brand-mark" aria-hidden="true">
          <span className="brand-mark__orb" />
          <span className="brand-mark__cloud" />
        </span>
        <span className="brand-name">Atmos</span>
      </div>

      <div className="brand-header__actions">
        <span className="live-indicator">
          <span className="live-indicator__dot" aria-hidden="true" />
          Moscow live
        </span>
        {onRefresh && (
          <button
            className="icon-button"
            type="button"
            onClick={onRefresh}
            disabled={isRefreshing}
            aria-label={isRefreshing ? 'Refreshing forecast' : 'Refresh forecast'}
          >
            <RefreshIcon className={isRefreshing ? 'is-spinning' : ''} />
          </button>
        )}
      </div>
    </header>
  )
}
