export function setKitEditTab(section: string): number {
  return KitEditTabMapper[section]
}

export const KitEditTabMapper = {
  undefined: 0,
  'catalog-assignments': 1,
}

export const TabIndexMapper = {
  1: 'catalog-assignments',
}
