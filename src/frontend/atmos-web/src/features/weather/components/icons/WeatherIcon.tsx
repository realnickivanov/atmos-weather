import { getWeatherKind } from '../../utils/condition'

interface WeatherIconProps {
  code: number
  isDay: boolean
  className?: string
}

export function WeatherIcon({ code, isDay, className = '' }: WeatherIconProps) {
  const kind = getWeatherKind(code)
  const classes = `weather-icon weather-icon--${kind} ${className}`.trim()

  return (
    <svg
      className={classes}
      viewBox="0 0 96 96"
      fill="none"
      aria-hidden="true"
      focusable="false"
    >
      {kind === 'clear' && (isDay ? <Sun /> : <Moon />)}
      {kind === 'cloud' && <CloudWithSky isDay={isDay} />}
      {kind === 'rain' && <RainCloud isDay={isDay} />}
      {kind === 'snow' && <SnowCloud isDay={isDay} />}
      {kind === 'storm' && <StormCloud />}
      {kind === 'fog' && <FogCloud />}
    </svg>
  )
}

function Sun() {
  return (
    <g className="weather-icon__sun">
      <circle cx="48" cy="48" r="19" />
      <path d="M48 12v10M48 74v10M12 48h10M74 48h10M22.5 22.5l7 7M66.5 66.5l7 7M73.5 22.5l-7 7M29.5 66.5l-7 7" />
    </g>
  )
}

function Moon() {
  return (
    <g className="weather-icon__moon">
      <path d="M67 61.5A28 28 0 0 1 34.5 25 28.5 28.5 0 1 0 67 61.5Z" />
      <circle cx="70" cy="25" r="3" />
      <circle cx="76" cy="39" r="2" />
    </g>
  )
}

function CloudWithSky({ isDay }: { isDay: boolean }) {
  return (
    <>
      <g className={isDay ? 'weather-icon__sun weather-icon__sky' : 'weather-icon__moon weather-icon__sky'}>
        {isDay ? <circle cx="60" cy="33" r="17" /> : <path d="M70 42A20 20 0 0 1 50 18a20 20 0 1 0 20 24Z" />}
      </g>
      <Cloud />
    </>
  )
}

function RainCloud({ isDay }: { isDay: boolean }) {
  return (
    <>
      <CloudWithSky isDay={isDay} />
      <g className="weather-icon__rain">
        <path d="m31 70-4 11M48 70l-4 11M65 70l-4 11" />
      </g>
    </>
  )
}

function SnowCloud({ isDay }: { isDay: boolean }) {
  return (
    <>
      <CloudWithSky isDay={isDay} />
      <g className="weather-icon__snow">
        <path d="M30 72v12M24.8 75l10.4 6M35.2 75l-10.4 6M48 72v12M42.8 75l10.4 6M53.2 75l-10.4 6M66 72v12M60.8 75l10.4 6M71.2 75l-10.4 6" />
      </g>
    </>
  )
}

function StormCloud() {
  return (
    <>
      <Cloud />
      <path className="weather-icon__lightning" d="M50 62 39 79h10l-4 13 16-21H51l7-9H50Z" />
    </>
  )
}

function FogCloud() {
  return (
    <>
      <Cloud />
      <g className="weather-icon__fog">
        <path d="M22 71h52M29 80h38M38 89h20" />
      </g>
    </>
  )
}

function Cloud() {
  return (
    <path
      className="weather-icon__cloud"
      d="M23 62.5a13.5 13.5 0 0 1 9.3-12.8A21.5 21.5 0 0 1 73 47.8a14.5 14.5 0 0 1-2 28.9H37a14 14 0 0 1-14-14.2Z"
    />
  )
}
