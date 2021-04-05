import { AssetMetadata } from './AssetMetadata';
import { History } from './History';

export interface Asset {
    ID?: string
    Title?: string
    Active?: boolean
    Url?: string
    Type?: 'Image' | 'Text' | 'Audio' | 'Video' | 'Presentation' | 'SpreadSheet' | 'PDF' | 'Compressed' | 'Code' | 'JSON' | 'Markup' | 'Unknown'
    Tags?: string[]
    FileName?: string
    Metadata?: AssetMetadata
    History?: History
}