export function getPsHeight(additionalClass = ''): number {
  /* The height of perfect scroll containers is typically dependent on certain items 
  that are always in the layout which are given the class name 'base-layout-item' in 
  addition to items that are specific to calculating the height of that specific perfect 
  scroll container.*/
  const baseLayoutItems = Array.from(
    document.getElementsByClassName('base-layout-item')
  )
  const additionalItems = additionalClass
    ? Array.from(document.getElementsByClassName(additionalClass))
    : []
  let totalHeight = 0
  ;[...baseLayoutItems, ...additionalItems].forEach((div) => {
    // div does contain the property 'offsetHeight, but typescript throws error
    totalHeight += (div as any).offsetHeight
  })
  return window.innerHeight - totalHeight
}

export function getScreenSizeBreakPoint(): string {
  const map = {
    xs: 575,
    sm: 767,
    md: 991,
    lg: 1199,
  }
  const innerWidth = window.innerWidth

  if (innerWidth < map.xs) {
    return 'xs'
  } else if (innerWidth > map.xs && innerWidth < map.sm) {
    return 'sm'
  } else if (innerWidth > map.sm && innerWidth < map.md) {
    return 'md'
  } else if (innerWidth > map.md && innerWidth < map.lg) {
    return 'lg'
  } else if (innerWidth > map.lg) {
    return 'xl'
  }
}
