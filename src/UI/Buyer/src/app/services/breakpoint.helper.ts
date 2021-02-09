const map = {
  xs: 575,
  sm: 767,
  md: 991,
  lg: 1199,
}

export const getScreenSizeBreakPoint = (): string => {
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
