import { BuyerXp } from './BuyerXp';

export interface HSBuyer {
    xp?: BuyerXp
    ID?: string
    Name?: string
    DefaultCatalogID?: string
    Active?: boolean
    readonly DateCreated?: string
}