import { SuperHSMeProduct } from './SuperHSMeProduct';
import { ChiliSpec } from './ChiliSpec';

export interface MeChiliTemplate {
    Product?: SuperHSMeProduct
    TemplateSpecs?: ChiliSpec[]
    ChiliTemplateID?: string
    ChiliConfigID?: string
    Frame?: string
}