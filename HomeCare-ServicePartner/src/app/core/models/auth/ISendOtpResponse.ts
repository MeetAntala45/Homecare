export interface ISendOtpResponse {
    cooldownSeconds: number | null;
    isRateLimited: boolean;
}