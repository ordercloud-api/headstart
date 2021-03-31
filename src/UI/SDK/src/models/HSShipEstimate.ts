import { ShipEstimateXP } from './ShipEstimateXP';
import { HSShipMethod } from './HSShipMethod';
import { ShipEstimateItem } from './ShipEstimateItem';

export interface HSShipEstimate {
    xp?: ShipEstimateXP
    ShipMethods?: HSShipMethod[]
    ID?: string
    SelectedShipMethodID?: string
    ShipEstimateItems?: ShipEstimateItem[]
}