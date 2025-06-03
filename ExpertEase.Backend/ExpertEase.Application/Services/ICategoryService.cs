using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface ICategoryService
{
    Task<ServiceResponse<CategoryAdminDTO>> GetCategory(Guid id, CancellationToken cancellationToken = default); 
    Task<ServiceResponse<PagedResponse<CategoryAdminDTO>>> GetCategoriesAdmin(PaginationSearchQueryParams pagination,
        CancellationToken cancellationToken = default);
    Task<ServiceResponse<List<CategoryDTO>>> GetCategories(string? search = null,
        CancellationToken cancellationToken = default);
    Task<ServiceResponse<CategoryDTO>> GetCategoryForSpecialist(Guid categoryId, Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResponse<List<CategoryDTO>>> GetCategoriesForSpecialist(Guid specialistId, string? search = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddCategory(CategoryAddDTO category, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddCategoryToSpecialist(CategorySpecialistDTO category, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateCategory(CategoryUpdateDTO category, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteCategory(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteCategoryFromSpecialist(Guid categoryId, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);

}