using MentalHealthApis.DTOs.Blog;

namespace MentalHealthApis.Services
{
    public interface IBlogService
    {
        // Categories
        Task<List<BlogCategoryDto>> GetCategoriesAsync();
        Task<BlogCategoryDto> GetCategoryByIdAsync(int id);
        Task<BlogCategoryDto> GetCategoryBySlugAsync(string slug);
        Task<BlogCategoryDto> CreateCategoryAsync(CreateBlogCategoryDto dto);
        Task<BlogCategoryDto> UpdateCategoryAsync(int id, UpdateBlogCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);

        // Posts
        Task<PaginatedResultDto<BlogPostSummaryDto>> GetPostsAsync(BlogFilterDto filter);
        Task<BlogPostDto> GetPostByIdAsync(int id);
        Task<BlogPostDto> GetPostBySlugAsync(string slug);
        Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto, string authorId);
        Task<BlogPostDto> UpdatePostAsync(int id, UpdateBlogPostDto dto, string authorId);
        Task<bool> DeletePostAsync(int id, string authorId);
        Task<bool> PublishPostAsync(int id, string authorId);
        Task<bool> IncrementViewCountAsync(int id);

        // Comments
        Task<List<BlogCommentDto>> GetPostCommentsAsync(int postId);
        Task<BlogCommentDto> CreateCommentAsync(CreateBlogCommentDto dto, string authorId);
        Task<BlogCommentDto> UpdateCommentAsync(int id, UpdateBlogCommentDto dto, string authorId);
        Task<bool> DeleteCommentAsync(int id, string authorId);
        Task<bool> ApproveCommentAsync(int id);

        // Tags
        Task<List<string>> GetTagsAsync();
        Task<List<BlogPostSummaryDto>> GetPostsByTagAsync(string tag, int page = 1, int pageSize = 10);
    }
}
