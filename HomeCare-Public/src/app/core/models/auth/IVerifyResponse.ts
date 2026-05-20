export interface IVerifyOtpResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  isNewUser: boolean;
  referralCode?: string;
}
