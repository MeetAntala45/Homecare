export const PAYMENT_MESSAGES = {
    GENERIC_ERROR: 'Something went wrong. Please try again.',
    INCOMPLETE_STEPS: 'Please complete all steps before proceeding.',
  
    STRIPE: {
      CANCELLED: 'Payment was cancelled. You can try again anytime.',
      EXPIRED: 'Your Stripe session expired. Please try again.',
      REFUNDED: 'Your payment has been refunded. The booking was cancelled as payment was received after booking time expired.',
      FAILED: 'Payment failed. Please try again.',
      PENDING: 'You have 5 minutes to complete your payment on Stripe.',
      SESSION_FAIL: 'Failed to initiate payment.',
    },
  
    BOOKING: {
      CREATE_ERROR: 'Something went wrong. Please try again.',
      SLOT_UPDATE_SUCCESS: 'Slot updated successfully',
      SLOT_UPDATE_FAIL: 'Failed to update slot. Please try again.',
    },
  } as const;