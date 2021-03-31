import { SupplierFilterConfig } from './SupplierFilterConfig';
import { History } from './History';

export interface SupplierFilterConfigDocument {
    ID?: string
    Doc?: SupplierFilterConfig
    SchemaSpecUrl?: string
    History?: History
}