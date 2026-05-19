export const CUSTOMER_MESSAGES = {
    LOAD_FAILED: 'Failed to load customers.',
    GENERIC_ERROR: 'Something went wrong',
    LOAD_TO_FAILED_SERVICETYPE: 'Failed to load service types',
    FAILED_TO_LOAD_PARTNER: "   Failed to load partners",
    DIALOG: {
        ADD_TITLE: 'Add Customer',
        ADD_SUBMIT: 'Save',
    },

    DELETE: {
        body: (name: string): string =>
            `Are you sure you want to delete <strong>${name}</strong>?`,
    },

    BLOCK: {
        body: (name: string): string =>
            `Are you sure you want to block <strong>${name}</strong>?`,
    },
    COMPLETE_BOOKING_DIALOG: "Are you sure you want to mark this booking as <strong>Completed</strong>?",
    DELETE_BOOKING_DIALOG: "Are you sure you want to <strong>Delete</strong> this booking?"
} as const;
export const BOOKING_MESSAGES = {
    CHANGE_SUCCESS: 'Expert changed successfully',
    CHANGE_ERROR: 'Failed to change expert',

    COMPLETE_SUCCESS: 'Booking marked as completed',
    COMPLETE_ERROR: 'Failed to complete booking',

    CANCEL_SUCCESS: 'Booking cancelled successfully',
    CANCEL_ERROR: 'Failed to cancel booking',

    DELETE_SUCCESS: 'Booking deleted successfully',
    DELETE_ERROR: 'Failed to delete booking',
    COMPLETE_BOOKING_DIALOG: "Are you sure you want to mark this booking as <strong>Completed</strong>?",
    DELETE_BOOKING_DIALOG: "Are you sure you want to <strong>Delete</strong> this booking?",
    NO_EXPERT_AVAILABLE :"No expert assigned to this booking."
};
