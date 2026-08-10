import { expect, test } from '@playwright/test'
import { weatherFixture } from '../src/test/weatherFixture'

test('presents the complete Moscow forecast', async ({ page }) => {
  await page.route('**/api/weather', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(weatherFixture),
    }),
  )

  await page.goto('/')

  await expect(page.getByRole('heading', { name: '24°' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Hourly forecast' })).toBeVisible()
  await expect(page.getByRole('heading', { name: '3-day forecast' })).toBeVisible()
  await expect(page.getByText('Moscow, Russia')).toBeVisible()
})

test('recovers from an API error through the retry action', async ({ page }) => {
  let shouldFail = true

  await page.route('**/api/weather', (route) => {
    if (shouldFail) {
      return route.fulfill({
        status: 502,
        contentType: 'application/problem+json',
        body: JSON.stringify({ detail: 'The latest weather information could not be retrieved.' }),
      })
    }

    return route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(weatherFixture),
    })
  })

  await page.goto('/')

  const errorAlert = page.getByRole('alert')
  const retryButton = page.getByRole('button', { name: 'Try again' })

  await expect(errorAlert).toBeVisible()
  await expect(retryButton).toBeEnabled()

  shouldFail = false
  await retryButton.click()

  await expect(page.getByRole('heading', { name: '24°' })).toBeVisible()
  await expect(errorAlert).toBeHidden()
})
