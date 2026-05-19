
export interface IPartnerAssignedService {
    bookingId:      number;
    customerId:     number;
    serviceName:    string;
    customerName:   string;
    dateTime:       string;
    serviceAddress: string;
    statusId:       number;
    status:         string;   // "Pending" | "Completed" | "Cancelled"
  }
  
  export interface IPartnerAssignedServiceFilter {
    pageNumber: number;
    pageSize:   number;
    sortBy:     string;
    sortOrder:  string;
    date:       string | null;   // "yyyy-MM-dd"
    time:       string | null;   // "HH:mm"
    status:     string;
  }