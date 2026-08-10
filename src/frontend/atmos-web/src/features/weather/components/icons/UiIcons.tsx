import type { SVGProps } from 'react'

type IconProps = SVGProps<SVGSVGElement>

const common = {
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 1.8,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
  'aria-hidden': true,
  focusable: false,
}

export function LocationIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0Z" />
      <circle cx="12" cy="10" r="2.6" />
    </svg>
  )
}

export function DropletIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M12 3S6 9.7 6 14a6 6 0 0 0 12 0c0-4.3-6-11-6-11Z" />
    </svg>
  )
}

export function WindIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M3 8h11.5a2.5 2.5 0 1 0-2.3-3.5" />
      <path d="M3 12h16a2 2 0 1 1-1.7 3" />
      <path d="M3 16h8" />
    </svg>
  )
}

export function GaugeIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M4.6 18a8 8 0 1 1 14.8 0" />
      <path d="m12 13 4-4" />
      <path d="M7 18h10" />
    </svg>
  )
}

export function EyeIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M2.5 12S6 6.5 12 6.5 21.5 12 21.5 12 18 17.5 12 17.5 2.5 12 2.5 12Z" />
      <circle cx="12" cy="12" r="2.5" />
    </svg>
  )
}

export function RainIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M7 11.5a4.5 4.5 0 0 1 8.6-1.8A3.7 3.7 0 1 1 17.3 17H7.5A2.8 2.8 0 0 1 7 11.5Z" />
      <path d="m9 19-1 2M13 19l-1 2M17 19l-1 2" />
    </svg>
  )
}

export function RefreshIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M20 7v5h-5" />
      <path d="M18.6 16A8 8 0 1 1 20 12" />
    </svg>
  )
}

export function ArrowIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M5 12h14M14 7l5 5-5 5" />
    </svg>
  )
}

export function SunriseIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M4 18h16" />
      <path d="M6 14a6 6 0 0 1 12 0" />
      <path d="M12 3v3M4.9 6.9 7 9M19.1 6.9 17 9" />
    </svg>
  )
}

export function SunsetIcon(props: IconProps) {
  return (
    <svg {...common} {...props}>
      <path d="M4 18h16" />
      <path d="M6 14a6 6 0 0 1 12 0" />
      <path d="M12 8V3M9.5 5.5 12 8l2.5-2.5" />
    </svg>
  )
}
