import { SuperHSProduct } from './SuperHSProduct';
import { ChiliSpec } from './ChiliSpec';

export interface ChiliTemplate {
    Product?: SuperHSProduct
    TemplateSpecs?: ChiliSpec[]
    ChiliTemplateID?: string
    ChiliConfigID?: string
    Frame?: string
}