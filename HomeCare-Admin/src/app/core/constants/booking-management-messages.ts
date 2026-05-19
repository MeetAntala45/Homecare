export const BOOKING_MANAGEMENT_MESSAGES = {
  LOAD_FAILED: 'Failed to load bookings.',
  DELETE_FAILED: 'Failed to delete customer.',
  LOAD_DETAILS_FAILED: 'Failed to load booking details.',
  LOAD_PARTNERS_FAILED: 'Failed to load available partners.',
  LOAD_SERVICE_TYPES_FAILED: 'Failed to load service types.',
  COMPLETE_CONFIRMATION: 'Are you sure you want to mark this booking as <strong>Completed</strong>?',
  DELETE_CONFIRMATION: `Are you sure you want to <strong>Delete</strong> this booking?`,
  NO_EXPERT: 'No expert assigned to this booking.',
  SIGNALR_REJOIN_FAILED : 'Failed to rejoin admin group',
  SIGNALR_CONNECTION_FAILED : 'SignalR connection error'

} as const;
