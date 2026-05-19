export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

export interface AdminProfile {
  name: string;
  role: string;
  mobileNumber: string;
  email: string;
  address: string;
  profileImage: string;
}

export interface UpdateContactPayload {
  mobileNumber: string;
  email: string;
  address: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}