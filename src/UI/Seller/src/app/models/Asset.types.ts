export type AssetType = "image" | "document";

// Can be eventually replaced by SDK
export interface DocumentAsset {
    FileName: string,
    Url: string
}

// Can be eventually replaced by SDK
export interface ImageAsset {
    Url: string,
    ThumbnailUrl: string
}