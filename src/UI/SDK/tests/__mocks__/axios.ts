function successResponse() {
  return Promise.resolve({
    data: {},
  })
}

const mockAxios = {
  create: () => {},
  get: jest.fn(() => Promise.resolve({ data: {} })),
  put: jest.fn(() => Promise.resolve({ data: {} })),
  post: jest.fn(() => Promise.resolve({ data: {} })),
  patch: jest.fn(() => Promise.resolve({ data: {} })),
  delete: jest.fn(() => Promise.resolve({ data: {} })),
  interceptors: {
    request: {
      use: jest.fn(),
    },
  },
}

mockAxios.create = jest.fn(() => mockAxios)

export default mockAxios
