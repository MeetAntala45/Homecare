import { PagedResult } from "../paged-result";

export interface IPaymentPagedResult<T> extends PagedResult<T> {
    minAmount: number;
    maxAmount: number;
}