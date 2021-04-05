import { ProductXp } from './ProductXp';
import { Inventory } from './Inventory';

export interface HSProduct {
    xp?: ProductXp
    readonly OwnerID?: string
    DefaultPriceScheduleID?: string
    AutoForward?: boolean
    ID?: string
    Name?: string
    Description?: string
    QuantityMultiplier?: number
    ShipWeight?: number
    ShipHeight?: number
    ShipWidth?: number
    ShipLength?: number
    Active?: boolean
    readonly SpecCount?: number
    readonly VariantCount?: number
    ShipFromAddressID?: string
    Inventory?: Inventory
    DefaultSupplierID?: string
}