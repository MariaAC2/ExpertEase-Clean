namespace ExpertEase.Application.Responses;

/// <summary>
/// This class encapsulated the response for a pagination request. 
/// </summary>
public class PagedResponse<T>(int page, int pageSize, int totalCount, List<T> data)
{
    /// <summary>
    /// Page is the number of the page.
    /// </summary>
    public int Page { get; set; } = page;

    /// <summary>
    /// PageSize is the maximum number of entries on each page.
    /// </summary>
    public int PageSize { get; set; } = pageSize;

    /// <summary>
    /// TotalCount is the maximum number of entries that can be retrieved form the backend.
    /// It should be used by the client to determine the total number of pages for a given page size. 
    /// </summary>
    public int TotalCount { get; set; } = totalCount;

    /// <summary>
    /// This is the collection of entries corresponding to the page requested.
    /// </summary>
    public List<T> Data { get; set; } = data;
}

public static class PagedResponseExtension
{
    public static PagedResponse<TOut> Map<TIn, TOut>(this PagedResponse<TIn> response, Func<TIn, TOut> selector) =>
        new(response.Page, response.PageSize, response.TotalCount, response.Data.Select(selector).ToList());
}