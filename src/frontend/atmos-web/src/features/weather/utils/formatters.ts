const timeFormatters = new Map<string, Intl.DateTimeFormat>()
const dateFormatters = new Map<string, Intl.DateTimeFormat>()
const hourFormatters = new Map<string, Intl.DateTimeFormat>()

export function formatTemperature(value: number): string {
  return `${Math.round(value)}°`
}

export function formatLocationTime(iso: string, timeZone: string): string {
  return getTimeFormatter(timeZone).format(new Date(iso))
}

export function formatLocationDate(iso: string, timeZone: string): string {
  return getDateFormatter(timeZone).format(new Date(iso))
}

export function formatHour(iso: string, timeZone: string): string {
  return getHourFormatter(timeZone).format(new Date(iso))
}

export function formatDayName(date: string): string {
  return new Intl.DateTimeFormat('en-US', {
    weekday: 'long',
    timeZone: 'UTC',
  }).format(dateAtNoonUtc(date))
}

export function formatShortDate(date: string): string {
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    timeZone: 'UTC',
  }).format(dateAtNoonUtc(date))
}

export function getDateKey(iso: string): string {
  return iso.slice(0, 10)
}

export function formatPercent(value: number): string {
  return `${Math.round(value)}%`
}

export function formatDecimal(value: number, suffix: string): string {
  return `${new Intl.NumberFormat('en-US', { maximumFractionDigits: 1 }).format(value)}${suffix}`
}

function getHourFormatter(timeZone: string): Intl.DateTimeFormat {
  const cached = hourFormatters.get(timeZone)
  if (cached) return cached

  const formatter = new Intl.DateTimeFormat('en-US', {
    hour: 'numeric',
    hour12: true,
    timeZone,
  })
  hourFormatters.set(timeZone, formatter)
  return formatter
}

function getTimeFormatter(timeZone: string): Intl.DateTimeFormat {
  const cached = timeFormatters.get(timeZone)
  if (cached) return cached

  const formatter = new Intl.DateTimeFormat('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
    timeZone,
  })
  timeFormatters.set(timeZone, formatter)
  return formatter
}

function getDateFormatter(timeZone: string): Intl.DateTimeFormat {
  const cached = dateFormatters.get(timeZone)
  if (cached) return cached

  const formatter = new Intl.DateTimeFormat('en-US', {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    timeZone,
  })
  dateFormatters.set(timeZone, formatter)
  return formatter
}

function dateAtNoonUtc(date: string): Date {
  return new Date(`${date}T12:00:00Z`)
}
