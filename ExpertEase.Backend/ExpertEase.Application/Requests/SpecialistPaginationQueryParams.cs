using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.Requests;

/// <summary>
/// Use this class to get the pagination and search query parameters from the HTTP request.
/// You can extend the class to add more parameters to the pagination query.
/// </summary>
public class SpecialistPaginationQueryParams : PaginationQueryParams
{
    /// <summary>
    /// Search term to filter results by name, email, phone number, or address.
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Filter specialists by category ID.
    /// </summary>
    public Guid? CategoryId { get; set; }
    
    /// <summary>
    /// Filter specialists by category name.
    /// </summary>
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// Minimum rating filter (inclusive).
    /// </summary>
    [Range(0, 5, ErrorMessage = "Minimum rating must be between 0 and 5")]
    public int? MinRating { get; set; }
    
    /// <summary>
    /// Maximum rating filter (inclusive).
    /// </summary>
    [Range(0, 5, ErrorMessage = "Maximum rating must be between 0 and 5")]
    public int? MaxRating { get; set; }
    
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