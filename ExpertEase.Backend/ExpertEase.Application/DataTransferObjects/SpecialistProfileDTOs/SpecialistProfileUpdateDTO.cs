using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

public class SpecialistProfileUpdateDTO
{
    [Required]
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public int? YearsExperience { get; set; }
    public string? Description { get; set; } = null!;
    
    // ADD CATEGORIES SUPPORT
    public List<Guid>? CategoryIds { get; set; }
    
    public List<string>? ExistingPortfolioPhotoUrls { get; set; }
    
    public List<string>? PhotoIdsToRemove { get; set; }
}

// Create a separate DTO for form data with files
public class SpecialistProfileUpdateFormDTO
{
    [Required]
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public int? YearsExperience { get; set; }
    public string? Description { get; set; }
    
    // ADD CATEGORIES SUPPORT
    public Guid[]? CategoryIds { get; set; }
    
    // New photos to upload
    public IFormFile[]? NewPortfolioPhotos { get; set; }
    
    // URLs of existing photos to keep
    public string[]? ExistingPortfolioPhotoUrls { get; set; }
    
    // Photo IDs to remove
    public string[]? PhotoIdsToRemove { get; set; }
}