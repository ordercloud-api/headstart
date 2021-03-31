import { LineItemStatusChange } from './LineItemStatusChange';
import { SuperHSShipment } from './SuperHSShipment';

export interface LineItemStatusChanges {
    Status?: 'Complete' | 'Submitted' | 'Open' | 'Backordered' | 'Canceled' | 'CancelRequested' | 'Returned' | 'ReturnRequested'
    Changes?: LineItemStatusChange[]
    SuperShipment?: SuperHSShipment
}