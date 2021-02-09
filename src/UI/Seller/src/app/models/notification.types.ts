export interface MonitoredProductFieldModifiedNotificationDocument {
  ID: string
  Doc: MonitoredProductFieldModifiedNotification
}

export interface MonitoredProductFieldModifiedNotification {
  Supplier: NotificationUser
  Product: NotificationProduct
  Status: NotificationStatus
  History: NotificationHistory
}

export interface NotificationUser {
  ID: string
  Name: string
}

export interface NotificationProduct {
  ID: string
  Name: string
  FieldModified: string
  PreviousValue: any
  CurrentValue: any
}

export interface NotificationHistory {
  ModifiedBy: NotificationUser
  ReviewedBy: NotificationUser
  DateModified: string
  DateReviewed: string
}

export enum NotificationStatus {
  SUBMITTED = 'SUBMITTED',
  ACCEPTED = 'ACCEPTED',
  DISMISSED = 'DISMISSED',
  REJECTED = 'REJECTED',
}
