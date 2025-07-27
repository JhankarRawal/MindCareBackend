using MentalHealthApis.DTOs.Blog;
using MentalHealthApis.Models;
using MentalHealthApis.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System; // Required for Exception
using System.Threading.Tasks; // Required for Task
using System.Collections.Generic; // Required for List
using Microsoft.Extensions.Logging; // Required for ILogger

namespace MentalHealthApis.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly ILogger<BlogController> _logger;

        public BlogController(IBlogService blogService, ILogger<BlogController> logger)
        {
            _blogService = blogService;
            _logger = logger;
        }

        // ... (All your existing endpoints like GetCategories, GetPosts, etc. remain unchanged)
        #region Existing Endpoints
        [HttpGet("categories")]
        public async Task<ActionResult<List<BlogCategoryDto>>> GetCategories()
        {
            try
            {
                var categories = await _blogService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blog categories");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<BlogCategoryDto>> GetCategory(int id)
        {
            try
            {
                var category = await _blogService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("categories/slug/{slug}")]
        public async Task<ActionResult<BlogCategoryDto>> GetCategoryBySlug(string slug)
        {
            try
            {
                var category = await _blogService.GetCategoryBySlugAsync(slug);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategorySlug}", slug);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogCategoryDto>> CreateCategory(CreateBlogCategoryDto dto)
        {
            try
            {
                var category = await _blogService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog category");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogCategoryDto>> UpdateCategory(int id, UpdateBlogCategoryDto dto)
        {
            try
            {
                var category = await _blogService.UpdateCategoryAsync(id, dto);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _blogService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("posts")]
        public async Task<ActionResult<List<BlogPostSummaryDto>>> GetPosts()
        {
            try
            {
                var posts = await _blogService.GetAllPublishedPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blog posts");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("posts/{id:int}")]
        public async Task<ActionResult<BlogPostDto>> GetPostById(int id)
        {
            try
            {
                var post = await _blogService.GetPostByIdAsync(id);
                if (post == null)
                    return NotFound();
                
                await _blogService.IncrementViewCountAsync(post.Id);

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post by ID {PostId}", id);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("posts/{slug}")]
        public async Task<ActionResult<BlogPostDto>> GetPost(string slug)
        {
            try
            {
                var post = await _blogService.GetPostBySlugAsync(slug);
                if (post == null)
                    return NotFound();
                
                await _blogService.IncrementViewCountAsync(post.Id);

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {PostSlug}", slug);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<BlogPostSummaryDto>>> GetAllPosts()
        {
            try
            {
                var posts = await _blogService.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all posts");
                return StatusCode(500, "Internal server error");
            }
        }

      [HttpPost("posts")]
    [Authorize]
    public async Task<ActionResult<BlogPostDto>> CreatePost([FromForm] CreateBlogPostDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var post = await _blogService.CreatePostAsync(dto, userId);
            return CreatedAtAction(nameof(GetPost), new { slug = post.Slug }, post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return StatusCode(500, "Internal server error");
        }
    }

        [HttpPut("posts/{id}")]
        [Authorize]
        public async Task<ActionResult<BlogPostDto>> UpdatePost(int id, UpdateBlogPostDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var post = await _blogService.UpdatePostAsync(id, dto, userId);
                if (post == null)
                    return NotFound();

                return Ok(post);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("posts/{id}")]
        [Authorize]
        public async Task<ActionResult> DeletePost(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _blogService.DeletePostAsync(id, userId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("posts/{id}/publish")]
        [Authorize]
        public async Task<ActionResult> PublishPost(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _blogService.PublishPostAsync(id, userId);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Post published successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing post {PostId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("tags")]
        public async Task<ActionResult<List<string>>> GetTags()
        {
            try
            {
                var tags = await _blogService.GetTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tags");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("tags/{tag}/posts")]
        public async Task<ActionResult<List<BlogPostSummaryDto>>> GetPostsByTag(string tag)
        {
            try
            {
                var posts = await _blogService.GetPostsByTagAsync(tag);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for tag {Tag}", tag);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("admin/posts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<BlogPostSummaryDto>>> GetAllPostsAdmin()
        {
            try
            {
                var posts = await _blogService.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all posts for admin");
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        // ========= ADD THIS ENTIRE NEW ENDPOINT =========
         #region Recommendations
        /// <summary>
        /// Gets a list of recommended blog posts based on the user's most recent journal sentiment.
        /// </summary>
        [HttpGet("recommendations")]
        [Authorize]
        // ========= CHANGE THE RETURN TYPE OF THE ACTION RESULT =========
        public async Task<ActionResult<List<BlogPostSummaryDto>>> GetBlogRecommendations()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
                {
                    return Unauthorized("User ID could not be determined from token.");
                }

                // ========= CALL THE NEW SERVICE METHOD =========
                var recommendations = await _blogService.GetRecommendedPostsForUserAsync(userId);

                // This now correctly returns a flat list of posts.
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating blog recommendations.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
        #endregion
    }
}