export interface IConfirmDialog {
    title: string;
    message: string;
    confirmLabel: string;
    confirmColor?: 'primary' | 'warn';
    apiCall: () => any;
  }