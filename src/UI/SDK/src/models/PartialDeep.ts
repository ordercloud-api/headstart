// Typescript's Partial helper only goes one level deep
// However partial for OrderCloud requests means Partial for any nested sub-model as well
export type PartialDeep<T> = T extends object
  ? { [K in keyof T]?: PartialDeep<T[K]> }
  : T
