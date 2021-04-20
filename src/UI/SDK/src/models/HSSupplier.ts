import { SupplierXp } from './SupplierXp';

export interface HSSupplier {
    xp?: SupplierXp
    ID?: string
    Name?: string
    Active?: boolean
    readonly DateCreated?: string
}