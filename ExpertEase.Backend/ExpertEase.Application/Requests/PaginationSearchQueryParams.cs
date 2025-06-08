namespace ExpertEase.Application.Requests;

/// <summary>
/// This class extends the pagination query parameters and includes a search string to be used in querying the database.
/// </summary>
public class PaginationSearchQueryParams : PaginationQueryParams
{
    public string? Search { get; set; }
}

public class PaginationReviewFilterQueryParams : PaginationQueryParams
{
    public int? Rating { get; set; }
}
