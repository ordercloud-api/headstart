export function setProductEditTab(section: string): number {
  const tabIndex = ProductEditTabMapper[section] as number | undefined
  if (typeof tabIndex === 'undefined') {
    throw new Error(`Section ${section} has no tab index mapping defined`)
  }
  return tabIndex
}

export const ProductEditTabMapper = {
  undefined: 0,
  'catalog-assignments': 1,
  description: 2,
  price: 3,
  filters: 4,
  variants: 5,
  'images-and-documents': 6,
}

export const TabIndexMapper = {
  1: 'catalog-assignments',
  2: 'description',
  3: 'price',
  4: 'filters',
  5: 'variants',
  6: 'images-and-documents',
  7: 'related-products',
}
