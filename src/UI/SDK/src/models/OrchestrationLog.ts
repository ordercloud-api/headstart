import { ApiError } from './ApiError';

export interface OrchestrationLog {
    ErrorType?: 'WorkItemDefinition' | 'QueuedGetError' | 'CachedGetError' | 'DiffCalculationError' | 'ActionEvaluationError' | 'CacheUpdateError' | 'QueueCleanupError' | 'SyncCommandError' | 'CreateExistsError' | 'CreateGeneralError' | 'UpdateGeneralError' | 'PatchGeneralError' | 'GetGeneralError' | 'AuthenticateSupplierError' | 'NoRelatedOrderCloudOrderFound'
    Message?: string
    Level?: 'Progress' | 'Error' | 'Warn' | 'Success'
    ResourceId?: string
    RecordId?: string
    RecordType?: 'HydratedProduct' | 'Product' | 'PriceSchedule' | 'Spec' | 'SpecOption' | 'SpecProductAssignment' | 'ProductFacet' | 'Buyer' | 'User' | 'UserGroup' | 'Address' | 'CostCenter' | 'UserGroupAssignment' | 'AddressAssignment' | 'CatalogAssignment' | 'Catalog' | 'Supplier' | 'Order' | 'TemplateProductFlat'
    Action?: 'Ignore' | 'Create' | 'Update' | 'Patch' | 'Delete' | 'Get' | 'SyncShipments'
    OrderCloudErrors?: ApiError[]
}