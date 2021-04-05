
export interface Notification {
    Channel?: 'Club' | 'Staff'
    Action?: 'Update' | 'Delete' | 'Create'
    TimestampUtc?: string
    JsonData?: string
}