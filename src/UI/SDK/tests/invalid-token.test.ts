import mockAxios from 'axios'
import {
  Tokens,
  Auth,
  Products,
  Configuration,
  AccessToken,
} from '../src/index'
import { makeToken } from './utils'

const apiUrl = 'https://api.ordercloud.io/v1'
const testdata = {
  accessToken:
    'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c3IiOiJ0ZXN0YnV5ZXIiLCJjaWQiOiI5N2JiZjJjYy01OWQxLTQ0OWEtYjY3Yy1hZTkyNjJhZGQyODQiLCJ1IjoiMTkyMDU2MyIsInVzcnR5cGUiOiJidXllciIsInJvbGUiOlsiTWVBZGRyZXNzQWRtaW4iLCJNZUFkbWluIiwiTWVDcmVkaXRDYXJkQWRtaW4iLCJNZVhwQWRtaW4iLCJTaG9wcGVyIiwiQnV5ZXJSZWFkZXIiXSwiaXNzIjoiaHR0cHM6Ly9hdXRoLm9yZGVyY2xvdWQuaW8iLCJhdWQiOiJodHRwczovL2FwaS5vcmRlcmNsb3VkLmlvIiwiZXhwIjoxNTY1Mzk5NjE5LCJuYmYiOjE1NjUzNjM2MTl9.tuWzEMa4lH2zx4zrab3X4d1uTFFwEAs7pfOZ_yQHV14',
  refreshToken: 'f36ebba3-5218-4f34-9657-b8738730b735',
  accessTokenFromRefresh:
    'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c3IiOiJ0ZXN0YnV5ZXIiLCJjaWQiOiI5N2JiZjJjYy01OWQxLTQ0OWEtYjY3Yy1hZTkyNjJhZGQyODQiLCJ1IjoiMTkyMDU2MyIsInVzcnR5cGUiOiJidXllciIsInJvbGUiOlsiTWVBZGRyZXNzQWRtaW4iLCJNZUFkbWluIiwiTWVDcmVkaXRDYXJkQWRtaW4iLCJNZVhwQWRtaW4iLCJTaG9wcGVyIiwiQnV5ZXJSZWFkZXIiXSwiaXNzIjoiaHR0cHM6Ly9hdXRoLm9yZGVyY2xvdWQuaW8iLCJhdWQiOiJodHRwczovL2FwaS5vcmRlcmNsb3VkLmlvIiwiZXhwIjoxNTY1NDAwOTg5LCJuYmYiOjE1NjUzNjQ5ODl9.eitJK5A8a3JyYhBm_PGp9A93-AGSRDvbkoowA38eyIc',
  clientID: 'my-mock-clientid',
  productID: 'my-mock-productid',
  clientIDFromToken: 'client-id-from-token',
}

beforeEach(() => {
  jest.clearAllMocks() // cleans up any tracked calls before the next test
  jest.restoreAllMocks() // clears information for spies
  Tokens.RemoveAccessToken()
  Tokens.RemoveRefreshToken()
  // reset defaults for configuration
  Configuration.Set({
    baseApiUrl: 'https://api.ordercloud.io/v1',
    baseAuthUrl: 'https://auth.ordercloud.io/oauth/token',
    timeoutInMilliseconds: 10 * 1000,
    clientID: null,
  })
})

describe('has expired access token', () => {
  const tenMinutesAgoIn = Date.now() + 1000 * 60 * 10
  const expiredToken = makeToken(tenMinutesAgoIn, testdata.clientIDFromToken)
  beforeEach(() => {
    Tokens.SetAccessToken(expiredToken)
  })
  describe('AND has no refresh token', () => {
    test('should make call with expired access token', async () => {
      await Products.Delete(testdata.productID)
      expect(mockAxios.delete).toHaveBeenCalledTimes(1)
      expect(mockAxios.delete).toHaveBeenCalledWith(
        `${apiUrl}/products/${testdata.productID}`,
        {
          params: {},
          timeout: 10000,
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${expiredToken}`,
          },
        }
      )
    })
  })
  describe('AND has refresh token', () => {
    describe('AND has clientID config set', () => {
      test('should attempt to use refresh token', async () => {
        Tokens.SetRefreshToken(testdata.refreshToken)
        Configuration.Set({ clientID: testdata.clientID })

        const GetRefreshTokenSpy = jest.spyOn(Tokens, 'GetRefreshToken')
        const RefreshTokenSpy = jest
          .spyOn(Auth, 'RefreshToken')
          .mockImplementationOnce(() => {
            const response: AccessToken = {
              access_token: testdata.accessTokenFromRefresh,
              expires_in: 32000000000,
              token_type: 'bearer',
              refresh_token: undefined,
            }
            return Promise.resolve(response)
          })

        await Products.Delete(testdata.productID)

        expect(mockAxios.delete).toHaveBeenCalledTimes(1)
        expect(GetRefreshTokenSpy).toHaveBeenCalledTimes(1)
        expect(RefreshTokenSpy).toHaveBeenCalledTimes(1)
        expect(RefreshTokenSpy).toHaveBeenCalledWith(
          testdata.refreshToken,
          testdata.clientID
        )
        expect(mockAxios.delete).toHaveBeenCalledWith(
          `${apiUrl}/products/${testdata.productID}`,
          {
            params: {},
            timeout: 10000,
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${testdata.accessTokenFromRefresh}`,
            },
          }
        )
      })
    })
    describe('AND has no clientID config set', () => {
      test('should attempt to use refresh token with clientid from parsing token', async () => {
        Tokens.SetRefreshToken(testdata.refreshToken)
        const GetRefreshTokenSpy = jest.spyOn(Tokens, 'GetRefreshToken')
        const RefreshTokenSpy = jest
          .spyOn(Auth, 'RefreshToken')
          .mockImplementationOnce(() => {
            const response: AccessToken = {
              access_token: testdata.accessTokenFromRefresh,
              expires_in: 32000000000,
              token_type: 'bearer',
              refresh_token: undefined,
            }
            return Promise.resolve(response)
          })

        await Products.Delete(testdata.productID)

        expect(mockAxios.delete).toHaveBeenCalledTimes(1)
        expect(GetRefreshTokenSpy).toHaveBeenCalledTimes(1)
        expect(RefreshTokenSpy).toHaveBeenCalledTimes(1)
        expect(RefreshTokenSpy).toHaveBeenCalledWith(
          testdata.refreshToken,
          testdata.clientIDFromToken
        )
        expect(mockAxios.delete).toHaveBeenCalledWith(
          `${apiUrl}/products/${testdata.productID}`,
          {
            params: {},
            timeout: 10000,
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${testdata.accessTokenFromRefresh}`,
            },
          }
        )
      })
    })
  })
})

describe('has no access token', () => {
  describe('AND has no refresh token', () => {
    test('should make call with no access token set', async () => {
      await Products.Delete(testdata.productID)
      expect(mockAxios.delete).toHaveBeenCalledTimes(1)
      expect(mockAxios.delete).toHaveBeenCalledWith(
        `${apiUrl}/products/${testdata.productID}`,
        {
          params: {},
          timeout: 10000,
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer `,
          },
        }
      )
    })
  })
  describe('AND has refresh token', () => {
    describe('AND has clientID config set', () => {
      test('should attempt to use refresh token', async () => {
        Tokens.SetRefreshToken(testdata.refreshToken)
        Configuration.Set({ clientID: testdata.clientID })

        const GetRefreshTokenSpy = jest.spyOn(Tokens, 'GetRefreshToken')
        const RefreshTokenSpy = jest
          .spyOn(Auth, 'RefreshToken')
          .mockImplementationOnce(() => {
            const response: AccessToken = {
              access_token: testdata.accessTokenFromRefresh,
              expires_in: 32000000000,
              token_type: 'bearer',
              refresh_token: undefined,
            }
            return Promise.resolve(response)
          })

        await Products.Delete(testdata.productID)

        expect(mockAxios.delete).toHaveBeenCalledTimes(1)
        expect(GetRefreshTokenSpy).toHaveBeenCalledTimes(1)
        expect(RefreshTokenSpy).toHaveBeenCalledTimes(1)
        expect(RefreshTokenSpy).toHaveBeenCalledWith(
          testdata.refreshToken,
          testdata.clientID
        )
        expect(mockAxios.delete).toHaveBeenCalledWith(
          `${apiUrl}/products/${testdata.productID}`,
          {
            params: {},
            timeout: 10000,
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${testdata.accessTokenFromRefresh}`,
            },
          }
        )
      })
    })
    describe('AND has no clientID config set', () => {
      test('should not attempt use refresh token (not enough info to try)', async () => {
        Tokens.SetRefreshToken(testdata.refreshToken)

        await Products.Delete(testdata.productID)

        expect(mockAxios.delete).toHaveBeenCalledTimes(1)
        expect(mockAxios.delete).toHaveBeenCalledWith(
          `${apiUrl}/products/${testdata.productID}`,
          {
            params: {},
            timeout: 10000,
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer `,
            },
          }
        )
      })
    })
  })
})

describe('has valid access token', () => {
  test('should use access token', async () => {
    const tenMinutesFromNow = Date.now() + 1000 * (60 * 10)
    const token = makeToken(tenMinutesFromNow, testdata.clientIDFromToken)
    Tokens.SetAccessToken(token)
    await Products.Delete(testdata.productID)
    expect(mockAxios.delete).toHaveBeenCalledTimes(1)
    expect(mockAxios.delete).toHaveBeenCalledWith(
      `${apiUrl}/products/${testdata.productID}`,
      {
        params: {},
        timeout: 10000,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
      }
    )
  })
})
