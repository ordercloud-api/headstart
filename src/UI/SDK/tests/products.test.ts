import mockAxios from 'axios'
import { Tokens, Products, Product } from '../src/index'
import { makeToken } from './utils'

const apiUrl = 'https://api.ordercloud.io/v1'
const validToken = makeToken()

beforeEach(() => {
  jest.clearAllMocks() // cleans up any tracked calls before the next test
  Tokens.RemoveAccessToken()
})

test('can create product', async () => {
  Tokens.SetAccessToken(validToken)
  const product: Product = {
    Name: 'Tennis Balls',
    ID: 'TB2038',
  }
  await Products.Create(product)
  expect(mockAxios.post).toHaveBeenCalledTimes(1)
  expect(mockAxios.post).toHaveBeenCalledWith(`${apiUrl}/products`, product, {
    params: {},
    timeout: 10000,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${validToken}`,
    },
  })
})

test('can patch product', async () => {
  Tokens.SetAccessToken(validToken)
  const productID = 'mockproductid'
  const partialProduct: Partial<Product> = {
    Description: 'This product is pretty sweet, trust me',
  }
  await Products.Patch(productID, partialProduct)
  expect(mockAxios.patch).toHaveBeenCalledTimes(1)
  expect(mockAxios.patch).toHaveBeenCalledWith(
    `${apiUrl}/products/${productID}`,
    partialProduct,
    {
      params: {},
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${validToken}`,
      },
    }
  )
})

test('can update product', async () => {
  Tokens.SetAccessToken(validToken)
  const productID = 'mockproductid'
  const product: Product = {
    Name: 'Tennis Balls',
    Description: 'This product is pretty sweet, trust me',
  }
  await Products.Save(productID, product)
  expect(mockAxios.put).toHaveBeenCalledTimes(1)
  expect(mockAxios.put).toHaveBeenCalledWith(
    `${apiUrl}/products/${productID}`,
    product,
    {
      params: {},
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${validToken}`,
      },
    }
  )
})

test('can delete product', async () => {
  Tokens.SetAccessToken(validToken)
  const productID = 'mockproductid'
  await Products.Delete(productID)
  expect(mockAxios.delete).toHaveBeenCalledTimes(1)
  expect(mockAxios.delete).toHaveBeenCalledWith(
    `${apiUrl}/products/${productID}`,
    {
      params: {},
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${validToken}`,
      },
    }
  )
})
