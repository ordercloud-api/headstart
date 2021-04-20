import { ProductXp } from './ProductXp';
import { HSPriceSchedule } from './HSPriceSchedule';
import { Inventory } from './Inventory';

export interface HSMeProduct {
    xp?: ProductXp
    readonly PriceSchedule?: HSPriceSchedule
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