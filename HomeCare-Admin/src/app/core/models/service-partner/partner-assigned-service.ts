
export interface IPartnerAssignedService {
    bookingId:      number;
    customerId:     number;
    serviceName:    string;
    customerName:   string;
    dateTime:       string;
    serviceAddress: string;
    statusId:       number;
    status:         string;
  }
  
  export interface IPartnerAssignedServiceFilter {
    pageNumber: number;
    pageSize:   number;
    sortBy:     string;
    sortOrder:  string;
    date:       string | null;
    time:       string | null;
    status:     string;
  }