using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.Requests;

/// <summary>
/// Use this class to get the pagination and search query parameters from the HTTP request.
/// Updated to match frontend structure.
/// </summary>
public class SpecialistPaginationQueryParams : PaginationSearchQueryParams
{
    public SpecialistFilterParams? Filters { get; set; } = new();
}

public class SpecialistFilterParams
{
    /// <summary>
    /// Filter specialists by category IDs.
    /// </summary>
    public List<string>? CategoryIds { get; set; }
    
    /// <summary>
    /// Minimum rating filter (inclusive).
    /// </summary>
    [Range(0, 5, ErrorMessage = "Minimum rating must be between 0 and 5")]
    public int? MinRating { get; set; }
    
    /// <summary>
    /// Sort specialists by rating in ascending or descending order.
    /// Accepted values: "asc", "desc"
    /// </summary>
    public string? SortByRating { get; set; }
    
    /// <summary>
    /// Filter specialists by years of experience range.
    /// Accepted values: "0-2", "2-5", "5-7", "7-10", "10+"
    /// </summary>
    public string? ExperienceRange { get; set; }
}