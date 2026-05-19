export interface IAdminNotification {
    id: number;
    bookingId: number;
    customerId: number;
    customerName: string;
    serviceName: string;
    paymentMethod: string;
    paymentMethodValue: number;
    slotDate: string;
    slotTime: string;
    amount: number;
    message: string;
    isRead: boolean;
    createdAt: string;
  }
  
  export interface IAdminNotificationPaged {
    items: IAdminNotification[];
    unreadCount: number;
  }