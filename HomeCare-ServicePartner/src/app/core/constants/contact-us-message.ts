export const CONTACT_US_MESSAGES = {
    success: {
      SUBMITTED: 'Contact submitted successfully'
    },
  
    error: {
      INVALID_EMAIL: 'Please enter a valid email address',
      INVALID_PHONE: 'Please enter a valid mobile number',
      INVALID_NAME: (field: string) => `${field} should contain only letters`,
      REQUIRED: (field: string) => `${field} is required`,
      MAX_LENGTH: (field: string) => `${field} is too long`,
      GENERIC: (field: string) => `Invalid ${field}`
    }
  };