export interface CarouselSlide {
  URL: string
  headerText: string
  bodyText: string
}

export interface CarouselSlideUpdate {
  prev: CarouselSlide
  new?: CarouselSlide
}
