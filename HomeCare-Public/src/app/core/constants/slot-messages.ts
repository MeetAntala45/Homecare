export const SLOT_BOOKING_MESSAGES = {
    modal: {
      TITLE: 'Book a Slot',
      SUBTITLE: 'When should our expert arrive?',
      SESSION_TITLE: 'Select Time Session',
      SLOT_TITLE: 'Available Time Slots',
      NO_SLOTS: 'No slots available for this session',
      NO_PARTNERS: 'No partners available for this service',
      SAVE_BTN: 'Confirm Slot',
      CANCEL_BTN: 'Cancel',
      SLOT_ALREADY_BOOKED: 'This slot is already booked',
      SUCCESS:'Slot selected successfully'
    },
    sessions: {
      MORNING: 'Morning',
      AFTERNOON: 'Afternoon',
      EVENING: 'Evening',
      NIGHT: 'Night',
    },
    errors: {
      LOAD_DATES_FAILED: 'Failed to load available dates',
      LOAD_SLOTS_FAILED: 'Failed to load available slots',
      SELECT_DATE: 'Please select a date',
      SELECT_SLOT: 'Please select a time slot',
      SLOT_CONFLICT: 'This slot was just booked by someone else. Please choose another time.',
    },
    api: {
      DATES_FETCHED: 'Dates fetched successfully',
      SLOTS_FETCHED: 'Slots fetched successfully',
    },
  };
  
  export const SESSION_CONFIG = [
    {
      key: 'Morning',
      label: 'Morning',
      range: '9 AM – 12 PM',
     
    },
    {
      key: 'Afternoon',
      label: 'Afternoon',
      range: '12 PM – 3 PM',
     
    },
    {
      key: 'Evening',
      label: 'Evening',
      range: '3 PM – 6 PM',
     
    },
    {
      key: 'Night',
      label: 'Night',
      range: '6 PM – 9 PM',
    
    },
  ];