import mockAxios from 'axios'
import { Tokens, Products } from '../src/index'
import { makeToken } from './utils'

const apiUrl = 'https://api.ordercloud.io/v1'
const testdata = {
  productID: 'my-mock-product-id',
}

beforeEach(() => {
  jest.clearAllMocks() // cleans up any tracked calls before the next test
  Tokens.RemoveImpersonationToken()
})

test('should use impersonation token if call As method', async () => {
  const impersonationToken = makeToken()
  'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c3IiOiJ0ZXN0YnV5ZXIiLCJjaWQiOiI5N2JiZjJjYy01OWQxLTQ0OWEtYjY3Yy1hZTkyNjJhZGQyODQiLCJ1IjoiMTkyMDU2MyIsInVzcnR5cGUiOiJidXllciIsInJvbGUiOlsiTWVBZGRyZXNzQWRtaW4iLCJNZUFkbWluIiwiTWVDcmVkaXRDYXJkQWRtaW4iLCJNZVhwQWRtaW4iLCJTaG9wcGVyIiwiQnV5ZXJSZWFkZXIiXSwiaXNzIjoiaHR0cHM6Ly9hdXRoLm9yZGVyY2xvdWQuaW8iLCJhdWQiOiJodHRwczovL2FwaS5vcmRlcmNsb3VkLmlvIiwiZXhwIjoxNTY1NDE2Njg1LCJuYmYiOjE1NjUzODA2ODV9.Fa35Zwz3dsolWgb2X2T4119RxZAGQiE2NoeRNeLaUek'
  Tokens.SetImpersonationToken(impersonationToken)
  await Products.As().Delete(testdata.productID)
  expect(mockAxios.delete).toHaveBeenCalledTimes(1)
  expect(mockAxios.delete).toHaveBeenCalledWith(
    `${apiUrl}/products/${testdata.productID}`,
    {
      params: {},
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${impersonationToken}`,
      },
    }
  )
})

test('should use passed in token if defined', async () => {
  const token = makeToken()
  await Products.Delete(testdata.productID, token)
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

test('should prioritize passed in token', async () => {
  const impersonationToken = makeToken()
  Tokens.SetImpersonationToken(impersonationToken)
  const token = makeToken()
  await Products.As().Delete(testdata.productID, token)
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
