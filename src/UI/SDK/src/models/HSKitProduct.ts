import { Product } from './Product';
import { Asset } from './Asset';
import { HSKitProductAssignment } from './HSKitProductAssignment';

export interface HSKitProduct {
    ID?: string
    Name?: string
    Product?: Product
    Images?: Asset[]
    Attachments?: Asset[]
    ProductAssignments?: HSKitProductAssignment
}