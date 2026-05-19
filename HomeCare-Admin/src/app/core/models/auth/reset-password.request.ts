export interface IResetPasswordRequest {
    token: string;
    newPassword: string;
    confirmPassword: string;
  }