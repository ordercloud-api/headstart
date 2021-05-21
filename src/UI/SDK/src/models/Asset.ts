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