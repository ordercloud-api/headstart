import { HSMeProduct } from './HSMeProduct';
import { Asset } from './Asset';
import { HSMeKitProductAssignment } from './HSMeKitProductAssignment';

export interface HSMeKitProduct {
    ID?: string
    Name?: string
    Product?: HSMeProduct
    Images?: Asset[]
    Attachments?: Asset[]
    ProductAssignments?: HSMeKitProductAssignment
}