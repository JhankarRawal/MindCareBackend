using MentalHealthApis.DTOs.Blog;

namespace MentalHealthApis.Services
{
    public interface IBlogService
    {
        // Categories
        Task<List<BlogCategoryDto>> GetCategoriesAsync();
        Task<BlogCategoryDto?> GetCategoryByIdAsync(int id);
        Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug);
        Task<BlogCategoryDto> CreateCategoryAsync(CreateBlogCategoryDto dto);
        Task<BlogCategoryDto?> UpdateCategoryAsync(int id, UpdateBlogCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);

        // Posts
        Task<List<BlogPostSummaryDto>> GetAllPostsAsync();               // returns all posts, no filtering
        Task<BlogPostDto?> GetPostByIdAsync(int id);
        Task<BlogPostDto?> GetPostBySlugAsync(string slug);
        Task<List<BlogPostSummaryDto>> GetAllPublishedPostsAsync();

        Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto, string authorId);
        Task<BlogPostDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto, string authorId);
        Task<bool> DeletePostAsync(int id, string authorId);
        Task<bool> PublishPostAsync(int id, string authorId);
        Task<bool> IncrementViewCountAsync(int id);

       

        // Tags
        Task<List<string>> GetTagsAsync();
        Task<List<BlogPostSummaryDto>> GetPostsByTagAsync(string tag);   // removed pagination params
    }
}
