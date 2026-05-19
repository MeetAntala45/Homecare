export const ADMIN_USERS_MESSAGES = {
    LOAD_FAILED: 'Failed to load admin users.',
    GENERIC_ERROR: 'Something went wrong',

    DIALOG: {
        ADD_TITLE: 'Add User',
        ADD_SUBMIT: 'Add',
        EDIT_TITLE: 'Update User',
        EDIT_SUBMIT: 'Update',
        CHANGE_PASSWORD_TITLE: 'Change Password',
        CHANGE_PASSWORD_SUBMIT: 'Change Password',
    },

    DELETE: {
        body: (name: string): string =>
            `Are you sure you want to delete the admin <strong>${name}</strong>?`,
    }
} as const;  