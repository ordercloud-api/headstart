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

export enum OrderReturnStatus {
  Unsubmitted = 'Unsubmitted',
  AwaitingApproval = 'AwaitingApproval',
  Declined = 'Declined',
  Open = 'Open',
  Completed = 'Completed',
  Canceled = 'Canceled',
}

export enum LineItemStatus {
  Complete = 'Complete',
  Submitted = 'Submitted',
  Open = 'Open',
  Backordered = 'Backordered',
}
