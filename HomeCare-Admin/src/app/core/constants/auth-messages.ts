export const AUTH_MESSAGES = {
  LOGIN: {
    INVALID_CREDENTIALS: 'Invalid email or password',
    GENERIC_ERROR: 'Something went wrong. Please try again.',
  },

  FORGOT_PASSWORD: {
    INVALID_EMAIL: 'Please enter a valid email address.',
    GENERIC_ERROR: 'Something went wrong. Please try again.',
  },

  RESET_PASSWORD: {
    INVALID_LINK: 'Invalid link',
    INVALID_EXPIRED: 'Invalid or expired reset link',
    PASSWORD_MISMATCH: 'Passwords do not match.',
    INVALID_DETAILS: 'Please enter valid details.',
    GENERIC_ERROR: 'Something went wrong. Please try again.',
  },
} as const;
