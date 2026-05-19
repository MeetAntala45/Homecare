export const SERVICE_PARTNER_MESSAGES = {
  LOAD_FAILED: 'Failed to load service partners.',
  LOAD_DETAIL_FAILED: 'Failed to load partner details.',
  APPROVED: 'Partner approved successfully.',
  APPROVE_FAILED: 'Failed to approve partner.',
  REJECTED: 'Partner rejected.',
  REJECT_FAILED: 'Failed to reject partner.',
  DOWNLOAD_FAILED: 'Failed to download file.',
  GENERIC_ERROR: 'Something went wrong.',
  DELETE_CONFIRM: 'Are you sure you want to delete this service partner?',

  TOGGLE_STATUS_DIALOG: {
    title: (isActive: boolean) => (isActive ? 'Deactivate Partner' : 'Activate Partner'),
    confirmLabel: (isActive: boolean) => (isActive ? 'Deactivate' : 'Activate'),
    body: (isActive: boolean, name: string) =>
      `Are you sure you want to ${isActive ? 'deactivate' : 'activate'} <strong>${name}</strong>?`,
  },
} as const;
