export const OFFERS_MESSAGES = {
  LOAD_FAILED: 'Failed to load offers.',
  GENERIC_ERROR: 'Something went wrong.',
  DELETE_CONFIRM: 'Are you sure you want to delete this offer?',

  CONDITION_TYPES: {
    LOAD_FAILED: 'Failed to load condition types.',
    CREATE_FAILED: 'Failed to create condition type.',
  },

  DIALOG: {
    ADD_TITLE: 'Add Offer',
    ADD_SUBMIT: 'Save',
    EDIT_TITLE: 'Update Offer',
    EDIT_SUBMIT: 'Update',
  },

  VALIDATION: {
    DUPLICATE_CONDITION: (label: string) =>
      `Each condition type can only be used once.`,
  },
} as const;
