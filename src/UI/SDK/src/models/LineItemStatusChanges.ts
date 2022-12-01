import { LineItemStatusChange } from './LineItemStatusChange';
import { SuperHSShipment } from './SuperHSShipment';

export interface LineItemStatusChanges {
    Status?: 'Complete' | 'Submitted' | 'Open' | 'Backordered'
    Changes?: LineItemStatusChange[]
    SuperShipment?: SuperHSShipment
}