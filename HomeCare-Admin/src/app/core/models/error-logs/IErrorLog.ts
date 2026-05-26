export interface IErrorLogList {
  id: number;
  exceptionType: string;
  message: string;
  path: string;
  httpMethod: string;
  statusCode: number;
  userId: number | null;
  userRole: string | null;
  occurredAt: string;
}

export interface IErrorLogDetail extends IErrorLogList {
  stackTrace: string | null;
}

export interface IErrorLogPagedResult {
  data: IErrorLogList[];
  totalCount: number;
}

export interface IErrorLogFilter {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortOrder: string;
  fromDate?: string | null;
  toDate?: string | null;
  statusCode?: number | null;
  exceptionType?: string;
  search?: string;
}
