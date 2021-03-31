import serialize from '../src/utils/paramsSerializer'

test('should serialize top-level params', () => {
  const params = {
    filters: {
      DateSubmitted: '>2018-04-20',
    },
  }
  expect(serialize(params)).toBe('DateSubmitted=%3E2018-04-20')
})

test('should handle filters', () => {
  const params = {
    filters: {
      LastName: 'Smith*',
    },
  }
  expect(serialize(params)).toBe('LastName=Smith*')
})

test('should handle arrays on filters', () => {
  const params = {
    filters: {
      xp: {
        Color: ['!red', '!blue'],
      },
    },
  }
  expect(serialize(params)).toBe('xp.Color=!red&xp.Color=!blue')
})

test('should handle mixed arrays and values filters', () => {
  const params = {
    filters: {
      FirstName: 'Bob',
      xp: {
        Color: ['!red', '!blue'],
      },
    },
  }
  expect(serialize(params)).toBe('FirstName=Bob&xp.Color=!red&xp.Color=!blue')
})

test('should throw if value is null', () => {
  const params = {
    filters: {
      FirstName: null,
    },
  }
  expect(() => serialize(params)).toThrowError(
    `Null is not a valid filter prop. Use negative filter "!" combined with wildcard filter "*" to define a filter for the absence of a value. \nex: an order list call with { xp: { hasPaid: '!*' } } would return a list of orders where xp.hasPaid is null or undefined\nhttps://ordercloud.io/features/advanced-querying#filtering`
  )
})

test('should ignore undefined values', () => {
  const params = {
    filters: {
      FirstName: 'Bob',
      LastName: undefined,
      xp: {
        FavoriteColor: 'red',
      },
    },
  }
  expect(serialize(params)).toBe('FirstName=Bob&xp.FavoriteColor=red')
})
