export const MESSAGES = {
  SERVICE: {
    LOAD_FAILED: 'Failed to load services',
    LOAD_DETAIL_FAILED: 'Failed to load service details.',
    DELETED: 'Service deleted successfully.',
    DELETE_FAILED: 'Failed to delete service.',
    AVAILABILITY_UPDATED: 'Availability updated successfully.',
    AVAILABILITY_FAILED: 'Failed to update availability.',
    OPERATION_FAILED: 'Operation failed.',
    LOAD_SUBCATEGORIES_FAILED: 'Failed to load sub categories.',
  },

  AVAILABILITY_DIALOG: {
    TITLE: 'Change Availability',
    body: (label: string, serviceName: string): string =>
      `<p class="text-secondary mb-0" style="font-size:14px;">
          Are you sure you want to <strong>${label}</strong> availability for
          <strong class="text-dark">${serviceName}</strong>?
        </p>`,
  },

  IMAGE: {
    UNSUPPORTED_TYPE: (name: string) => `"${name}" is not a supported image type.`,
    SIZE_EXCEEDED: (name: string) => `"${name}" exceeds the 5MB size limit.`,
  },

  VALIDATION: {
    SERVICE_NAME_REQUIRED: 'Service name is required',
    SELECT_SUB_CATEGORY: 'Select a sub category',
    DESCRIPTION_REQUIRED : 'Description is required',
    DURATION_INVALID: 'Must be greater than 0',
    PRICE_INVALID: 'Must be greater than 0',
    COMMISSION_INVALID: 'Must be between 0 and 100',
    SELECT_CATEGORY: 'Please select a category first',
    SUBCATEGORY_REQUIRED: 'Please add at least one subcategory'
  },

  DELETE: {
    body: (name: string): string => `Are you sure you want to delete <strong>${name}</strong>?`,
  },
} as const;
