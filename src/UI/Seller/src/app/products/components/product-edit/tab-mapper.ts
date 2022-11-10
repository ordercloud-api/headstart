import { invert } from 'lodash'

export function setProductEditTab(section: string): number {
  const map = invert(TabIndexMapper)
  const tabIndex = parseInt(map[section])
  if (typeof map[section] === 'undefined') {
    throw new Error(`Section ${section} has no tab index mapping defined`)
  }
  return tabIndex
}

export const TabIndexMapper = {
  0: 'undefined',
  1: 'catalog-assignments',
  2: 'description',
  3: 'price',
  4: 'filters',
  5: 'variants',
  6: 'images-and-documents',
  7: 'related-products',
}
