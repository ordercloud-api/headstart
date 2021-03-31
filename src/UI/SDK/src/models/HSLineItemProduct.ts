import { ProductXp } from './ProductXp';

export interface HSLineItemProduct {
    xp?: ProductXp
    ID?: string
    Name?: string
    Description?: string
    QuantityMultiplier?: number
    ShipWeight?: number
    ShipHeight?: number
    ShipWidth?: number
    ShipLength?: number
}