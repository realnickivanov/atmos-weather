import { useEffect, useState } from 'react'

const minuteInMilliseconds = 60_000
const schedulingToleranceMilliseconds = 25

export function useLiveClock(serverTimeIso: string): Date {
  const [now, setNow] = useState(() => parseServerTime(serverTimeIso))

  useEffect(() => {
    const serverTimeMilliseconds = Date.parse(serverTimeIso)
    if (!Number.isFinite(serverTimeMilliseconds)) {
      return undefined
    }

    const monotonicStart = performance.now()
    let timeoutId: number | undefined

    const updateClock = () => {
      const elapsedMilliseconds = performance.now() - monotonicStart
      const currentMilliseconds = serverTimeMilliseconds + elapsedMilliseconds
      setNow(new Date(currentMilliseconds))

      const millisecondsUntilNextMinute =
        minuteInMilliseconds -
        (currentMilliseconds % minuteInMilliseconds) +
        schedulingToleranceMilliseconds

      timeoutId = window.setTimeout(updateClock, millisecondsUntilNextMinute)
    }

    updateClock()

    return () => {
      if (timeoutId !== undefined) {
        window.clearTimeout(timeoutId)
      }
    }
  }, [serverTimeIso])

  return now
}

function parseServerTime(value: string): Date {
  const milliseconds = Date.parse(value)
  return Number.isFinite(milliseconds) ? new Date(milliseconds) : new Date(0)
}
