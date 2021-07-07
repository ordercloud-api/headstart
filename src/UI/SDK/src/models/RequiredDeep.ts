// Typescript's Required helper only goes one level deep
// However when we define a response from OrderCloud
// the property will exist in all nested objects
export declare type RequiredDeep<T> = T extends object
  ? {
      [K in keyof T]-?: RequiredDeep<T[K]>
    }
  : T
