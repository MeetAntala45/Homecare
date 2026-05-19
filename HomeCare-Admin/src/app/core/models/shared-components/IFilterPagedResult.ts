import { PagedResult } from "../paged-result";

export interface IFilterPagedResult<T> extends PagedResult<T> {
    min: number;
    max: number;
}