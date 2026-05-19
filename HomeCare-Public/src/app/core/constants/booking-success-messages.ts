export const BOOKING_SUCCESS_MESSAGES = {
    INVALID_BOOKING: 'Invalid booking. Redirecting...',
    FAILED_TO_DOWNLOAD_INVOICE: 'Failed to download invoice.',
    MOBILE_NUMBER_COPIED: 'Mobile number copied',

    BOOKING_DETAILS: {
        CONFIRM_BOOKING: 'Your booking is confirmed.',
        PAYMENT_EXPIRED: 'Your payment session expired. Please try again.',
        PAYMENT_REFUNDED: 'Your payment was refunded as booking was cancelled. Please try again.',
        PAYMENT_ERROR_MESSAGE: 'Something went wrong with your payment.',
        UNABLE_TO_VERIFY_PAYMENT: 'Unable to verify payment. Please try again.'
    }
} as const;