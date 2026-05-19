export interface ILoginResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  role: string;
}