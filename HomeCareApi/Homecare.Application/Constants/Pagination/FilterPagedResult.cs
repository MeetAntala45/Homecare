using System;

namespace Homecare.Application.Constants.Pagination;

public class FilterPagedResult<T> : PagedResult<T> {
    public int Min { get; set; }
    public int Max { get; set; }
}

