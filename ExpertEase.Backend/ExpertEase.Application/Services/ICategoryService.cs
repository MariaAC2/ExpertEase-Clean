using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface ICategoryService
{
    Task<ServiceResponse<CategoryDTO>> GetCategory(Guid id, CancellationToken cancellationToken = default); 
    Task<ServiceResponse<PagedResponse<CategoryDTO>>> GetCategories(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<CategoryDTO>> GetCategoryForSpecialist(string categoryName, Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<CategoryDTO>>> GetCategoriesForSpecialist(Guid specialistId, PaginationQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddCategory(CategoryAddDTO category, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddCategoryToSpecialist(string categoryName, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateCategory(CategoryUpdateDTO category, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteCategory(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteCategoryFromSpecialist(string categoryName, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);

}