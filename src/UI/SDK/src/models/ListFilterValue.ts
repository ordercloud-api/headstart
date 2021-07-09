export interface ListFilterValue {
    Term?: string
    Operator?: 'Equal' | 'GreaterThan' | 'LessThan' | 'GreaterThanOrEqual' | 'LessThanOrEqual' | 'NotEqual'
    WildcardPositions?: number[]
    HasWildcard?: boolean
}