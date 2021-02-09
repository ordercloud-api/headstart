export interface OrderProgress {
  StatusDisplay: string
  Value: number
  ProgressBarType: string
  Striped: boolean
  Animated: boolean
}

export enum OrderType {
  Quote = 'Quote',
  Standard = 'Standard',
}

export enum ShippingStatus {
  Shipped = 'Shipped',
  PartiallyShipped = 'PartiallyShipped',
  Canceled = 'Canceled',
  Processing = 'Processing',
  Backordered = 'Backordered',
}

export enum ClaimStatus {
  NoClaim = 'NoClaim',
  Pending = 'Pending',
  Complete = 'Complete',
}

export enum LineItemStatus {
  Complete = 'Complete',
  Submitted = 'Submitted',
  Open = 'Open',
  Backordered = 'Backordered',
  Canceled = 'Canceled',
  CancelRequested = 'CancelRequested',
  Returned = 'Returned',
  ReturnRequested = 'ReturnRequested',
}
