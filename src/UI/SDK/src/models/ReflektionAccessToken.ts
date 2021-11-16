export interface ReflektionAccessToken {
    accessToken?:string;
    accessTokenExpiry?: number;
    refreshToken?: string;
    refreshTokenExpiry?: number;
}