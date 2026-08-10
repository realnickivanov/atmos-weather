export type WeatherKind =
  | 'clear'
  | 'cloud'
  | 'fog'
  | 'rain'
  | 'snow'
  | 'storm'

export type WeatherMood =
  | 'clear-day'
  | 'clear-night'
  | 'clouds'
  | 'fog'
  | 'rain'
  | 'snow'
  | 'storm'

const stormCodes = new Set([1087, 1273, 1276, 1279, 1282])
const snowCodes = new Set([
  1066, 1069, 1072, 1114, 1117, 1168, 1171, 1204, 1207, 1210, 1213,
  1216, 1219, 1222, 1225, 1237, 1249, 1252, 1255, 1258, 1261, 1264,
])
const rainCodes = new Set([
  1063, 1150, 1153, 1180, 1183, 1186, 1189, 1192, 1195, 1198, 1201,
  1240, 1243, 1246,
])
const fogCodes = new Set([
  1012, 1015, 1018, 1021, 1024, 1027, 1030, 1033, 1036, 1039, 1042,
  1045, 1048, 1135, 1147,
])
const cloudCodes = new Set([1003, 1006, 1009])

export function getWeatherKind(code: number): WeatherKind {
  if (stormCodes.has(code)) return 'storm'
  if (snowCodes.has(code)) return 'snow'
  if (rainCodes.has(code)) return 'rain'
  if (fogCodes.has(code)) return 'fog'
  if (cloudCodes.has(code)) return 'cloud'
  return 'clear'
}

export function getWeatherMood(code: number, isDay: boolean): WeatherMood {
  const kind = getWeatherKind(code)

  if (kind === 'clear') {
    return isDay ? 'clear-day' : 'clear-night'
  }

  return kind === 'cloud' ? 'clouds' : kind
}
