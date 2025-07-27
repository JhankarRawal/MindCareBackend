using MentalHealthApis.DTOs.Blog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public interface IBlogService
    {
        // ... (all existing method definitions remain)
        #region Existing Methods
        Task<List<BlogCategoryDto>> GetCategoriesAsync();
        Task<BlogCategoryDto?> GetCategoryByIdAsync(int id);
        Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug);
        Task<BlogCategoryDto> CreateCategoryAsync(CreateBlogCategoryDto dto);
        Task<BlogCategoryDto?> UpdateCategoryAsync(int id, UpdateBlogCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<List<BlogPostSummaryDto>> GetAllPostsAsync();
        Task<BlogPostDto?> GetPostByIdAsync(int id);
        Task<BlogPostDto?> GetPostBySlugAsync(string slug);
        Task<List<BlogPostSummaryDto>> GetAllPublishedPostsAsync();
        Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto, string authorId);
        Task<BlogPostDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto, string authorId);
        Task<bool> DeletePostAsync(int id, string authorId);
        Task<bool> PublishPostAsync(int id, string authorId);
        Task<bool> IncrementViewCountAsync(int id);
        Task<List<string>> GetTagsAsync();
        Task<List<BlogPostSummaryDto>> GetPostsByTagAsync(string tag);
        #endregion

        // ========= ADD THESE NEW METHOD DEFINITIONS =========
        /// <summary>
        /// Gets blog categories that contain published posts tagged with any of the specified tags.
        /// </summary>
        Task<List<BlogPostSummaryDto>> GetRecommendedPostsForUserAsync(int userId);

        /// <summary>
        /// Generates a list of recommended blog categories based on a user's latest sentiment.
        /// </summary>
        // Task<List<BlogCategoryDto>> GetRecommendedCategoriesForUserAsync(int userId);
    }
}