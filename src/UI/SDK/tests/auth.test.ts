import mockAxios from 'axios'
import { Auth, SecurityProfile } from '../src/index'

const testdata = {
  authUrl: 'https://auth.ordercloud.io/oauth/token',
  username: '$crhistian', // handles special chars
  password: '87awesomesauce#$%^&', // handles special chars
  clientSecret: 'my-mock-secret',
  clientID: '12345678-1234-1C34-1234-6BAB2E6CB1F0',
  scope: ['BuyerAdmin', 'WebhookAdmin'],
  authHeaders: {
    'Content-Type': 'application/x-www-form-urlencoded',
    Accept: 'application/json',
  },
}

afterEach(() => {
  // cleans up any tracked calls before the next test
  jest.clearAllMocks()
})

const urlencode = encodeURIComponent
test('can auth with login', async () => {
  await Auth.Login(
    testdata.username,
    testdata.password,
    testdata.clientID,
    testdata.scope as SecurityProfile['Roles'][]
  )
  expect(mockAxios.post).toHaveBeenCalledTimes(1)
  const body = `grant_type=password&username=${urlencode(
    testdata.username
  )}&password=${urlencode(testdata.password)}&client_id=${
    testdata.clientID
  }&scope=${urlencode(testdata.scope.join(' '))}`
  expect(mockAxios.post).toHaveBeenCalledWith(testdata.authUrl, body, {
    headers: testdata.authHeaders,
  })
})

test('can auth with elevated login', async () => {
  await Auth.ElevatedLogin(
    testdata.clientSecret,
    testdata.username,
    testdata.password,
    testdata.clientID,
    testdata.scope as SecurityProfile['Roles'][]
  )
  expect(mockAxios.post).toHaveBeenCalledTimes(1)
  const body = `grant_type=password&scope=${urlencode(
    testdata.scope.join(' ')
  )}&client_id=${testdata.clientID}&username=${urlencode(
    testdata.username
  )}&password=${urlencode(testdata.password)}&client_secret=${urlencode(
    testdata.clientSecret
  )}`
  expect(mockAxios.post).toHaveBeenCalledWith(testdata.authUrl, body, {
    headers: testdata.authHeaders,
  })
})

test('can auth with client credentials', async () => {
  await Auth.ClientCredentials(
    testdata.clientSecret,
    testdata.clientID,
    testdata.scope as SecurityProfile['Roles'][]
  )
  expect(mockAxios.post).toHaveBeenCalledTimes(1)
  const body = `grant_type=client_credentials&scope=${urlencode(
    testdata.scope.join(' ')
  )}&client_id=${testdata.clientID}&client_secret=${testdata.clientSecret}`
  expect(mockAxios.post).toHaveBeenCalledWith(testdata.authUrl, body, {
    headers: testdata.authHeaders,
  })
})

test('can auth with refresh token', async () => {
  const refreshToken = 'mock-refresh-token'
  await Auth.RefreshToken(refreshToken, testdata.clientID)
  expect(mockAxios.post).toHaveBeenCalledTimes(1)
  const body = `grant_type=refresh_token&client_id=${testdata.clientID}&refresh_token=${refreshToken}`
  expect(mockAxios.post).toHaveBeenCalledWith(testdata.authUrl, body, {
    headers: testdata.authHeaders,
  })
})

test('can auth anonymous', async () => {
  await Auth.Anonymous(
    testdata.clientID,
    testdata.scope as SecurityProfile['Roles'][]
  )
  expect(mockAxios.post).toHaveBeenCalledTimes(1)

  const body = `grant_type=client_credentials&client_id=${
    testdata.clientID
  }&scope=${urlencode(testdata.scope.join(' '))}`
  expect(mockAxios.post).toHaveBeenCalledWith(testdata.authUrl, body, {
    headers: testdata.authHeaders,
  })
})
