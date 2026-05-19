export interface IPartnerNotification {
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
    status?: string
  }
  
  export interface IPartnerNotificationPaged {
    items: IPartnerNotification[];
    unreadCount: number;
  }