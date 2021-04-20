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

export type AssetType = "image" | "document";

export interface DocumentAsset {
    FileName?: string,
    Url?: string
}

export interface ImageAsset {
    Url?: string,
    ThumbnailUrl?: string
    Tags?: string[]
}

export interface FileData {
    File: File
    Filename?: string
  }