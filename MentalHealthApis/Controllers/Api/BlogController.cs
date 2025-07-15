using MentalHealthApis.DTOs.Blog;
using MentalHealthApis.Models;
using MentalHealthApis.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

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

            #region Categories

            /// <summary>
            /// Get all blog categories
            /// </summary>
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

            /// <summary>
            /// Get category by ID
            /// </summary>
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

            /// <summary>
            /// Get category by slug
            /// </summary>
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

            /// <summary>
            /// Create new category (Admin only)
            /// </summary>
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

            /// <summary>
            /// Update category (Admin only)
            /// </summary>
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

            /// <summary>
            /// Delete category (Admin only)
            /// </summary>
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

            #endregion

            #region Posts

            /// <summary>
            /// Get paginated blog posts with filtering
            /// </summary>
            [HttpGet("posts")]
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<BlogPostSummaryDto>>> GetPosts([FromQuery] BlogFilterDto filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (!filter.Status.HasValue)
                    filter.Status = PostStatus.Published;

                var posts = await _blogService.GetPostsAsync(filter);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blog posts");
                return StatusCode(500, "Internal server error");
            }
        }


        /// <summary>
        /// Get posts by category slug
        /// </summary>
        [HttpGet("posts/category/{categorySlug}")]
            public async Task<ActionResult<PaginatedResultDto<BlogPostSummaryDto>>> GetPostsByCategory(
                string categorySlug,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
            {
                try
                {
                    var filter = new BlogFilterDto
                    {
                        CategorySlug = categorySlug,
                        Status = PostStatus.Published,
                        Page = page,
                        PageSize = pageSize
                    };

                    var posts = await _blogService.GetPostsAsync(filter);
                    return Ok(posts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving posts for category {CategorySlug}", categorySlug);
                    return StatusCode(500, "Internal server error");
                }
            }

            /// <summary>
            /// Get featured posts
            /// </summary>
            [HttpGet("posts/featured")]
            public async Task<ActionResult<PaginatedResultDto<BlogPostSummaryDto>>> GetFeaturedPosts([FromQuery] int limit = 5)
            {
                try
                {
                    var filter = new BlogFilterDto
                    {
                        IsFeatured = true,
                        Status = PostStatus.Published,
                        PageSize = limit,
                        Page = 1
                    };

                    var posts = await _blogService.GetPostsAsync(filter);
                    return Ok(posts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving featured posts");
                    return StatusCode(500, "Internal server error");
                }
            }

            /// <summary>
            /// Get post by slug
            /// </summary>
            [HttpGet("posts/{slug}")]
            public async Task<ActionResult<BlogPostDto>> GetPost(string slug)
            {
                try
                {
                    var post = await _blogService.GetPostBySlugAsync(slug);
                    if (post == null)
                        return NotFound();

                    // Increment view count
                    await _blogService.IncrementViewCountAsync(post.Id);

                    return Ok(post);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving post {PostSlug}", slug);
                    return StatusCode(500, "Internal server error");
                }
            }

            /// <summary>
            /// Create new blog post (Authenticated users)
            /// </summary>
            [HttpPost("posts")]
            [Authorize]
            public async Task<ActionResult<BlogPostDto>> CreatePost(CreateBlogPostDto dto)
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

            /// <summary>
            /// Update blog post (Author or Admin only)
            /// </summary>
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

            /// <summary>
            /// Delete blog post (Author or Admin only)
            /// </summary>
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

            /// <summary>
            /// Publish post (Author or Admin only)
            /// </summary>
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

            /// <summary>
            /// Search posts
            /// </summary>
            [HttpGet("posts/search")]
            public async Task<ActionResult<PaginatedResultDto<BlogPostSummaryDto>>> SearchPosts(
                [FromQuery] string query,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
            {
                try
                {
                    var filter = new BlogFilterDto
                    {
                        Search = query,
                        Status = PostStatus.Published,
                        Page = page,
                        PageSize = pageSize
                    };

                    var posts = await _blogService.GetPostsAsync(filter);
                    return Ok(posts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching posts with query {SearchQuery}", query);
                    return StatusCode(500, "Internal server error");
                }
            }

            #endregion

            #region Comments

            /// <summary>
            /// Get comments for a post
            /// </summary>
            [HttpGet("posts/{postId}/comments")]
            public async Task<ActionResult<List<BlogCommentDto>>> GetPostComments(int postId)
            {
                try
                {
                    var comments = await _blogService.GetPostCommentsAsync(postId);
                    return Ok(comments);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                    return StatusCode(500, "Internal server error");
                }
            }

            /// <summary>
            /// Create comment (Authenticated users)
            /// </summary>
            [HttpPost("comments")]
            [Authorize]
            public async Task<ActionResult<BlogCommentDto>> CreateComment(CreateBlogCommentDto dto)
            {
                try
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized();

                    var comment = await _blogService.CreateCommentAsync(dto, userId);
                    return CreatedAtAction(nameof(GetPostComments), new { postId = comment.BlogPostId }, comment);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating comment");
                    return StatusCode(500, "Internal server error");
                }
            }

            /// <summary>
            /// Update comment (Author only)
            /// </summary>
            [HttpPut("comments/{id}")]
            [Authorize]
            public async Task<ActionResult<BlogCommentDto>> UpdateComment(int id, UpdateBlogCommentDto dto)
            {
                try
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized();

                    var comment = await _blogService.UpdateCommentAsync(id, dto, userId);
                    if (comment == null)
                        return NotFound();

                    return Ok(comment);
                }
                catch (UnauthorizedAccessException)
                {
                    return Forbid();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating comment {CommentId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }

            /// <summary>
            /// Delete comment (Author or Admin only)
            /// </summary>
            [HttpDelete("comments/{id}")]
            [Authorize]
            public async Task<ActionResult> DeleteComment(int id)
            {
                try
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized();

                    var result = await _blogService.DeleteCommentAsync(id, userId);
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
                    _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }

            /// <summary>
            /// Approve comment (Admin only)
            /// </summary>
            [HttpPut("comments/{id}/approve")]
            [Authorize(Roles = "Admin")]
            public async Task<ActionResult> ApproveComment(int id)
            {
                try
                {
                    var result = await _blogService.ApproveCommentAsync(id);
                    if (!result)
                        return NotFound();

                    return Ok(new { message = "Comment approved successfully" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error approving comment {CommentId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }

            #endregion

            #region Tags

            /// <summary>
            /// Get all tags
            /// </summary>
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

            /// <summary>
            /// Get posts by tag
            /// </summary>
            [HttpGet("tags/{tag}/posts")]
            public async Task<ActionResult<List<BlogPostSummaryDto>>> GetPostsByTag(
                string tag,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
            {
                try
                {
                    var posts = await _blogService.GetPostsByTagAsync(tag, page, pageSize);
                    return Ok(posts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving posts for tag {Tag}", tag);
                    return StatusCode(500, "Internal server error");
                }
            }

            #endregion

            //#region Admin Dashboard

            ///// <summary>
            ///// Get admin dashboard stats (Admin only)
            ///// </summary>
            //[HttpGet("admin/dashboard")]
            //[Authorize(Roles = "Admin")]
            //public async Task<ActionResult<object>> GetDashboardStats()
            //{
            //    try
            //    {
            //        var filter = new BlogFilterDto { PageSize = 1000 };
            //        var allPosts = await _blogService.GetPostsAsync(filter);

            //        var stats = new
            //        {
            //            TotalPosts = allPosts.TotalCount,
            //            PublishedPosts = allPosts.Items.Count(p => p.Status == "Published"),
            //            DraftPosts = allPosts.Items.Count(p => p.Status == "Draft"),
            //            TotalViews = allPosts.Items.Sum(p => p.ViewCount),
            //            TotalComments = allPosts.Items.Sum(p => p.CommentCount),
            //            Categories = (await _blogService.GetCategoriesAsync()).Count,
            //            Tags = (await _blogService.GetTagsAsync()).Count,
            //            RecentPosts = allPosts.Items.Take(5).ToList()
            //        };

            //        return Ok(stats);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error retrieving dashboard stats");
            //        return StatusCode(500, "Internal server error");
            //    }
            //}

            /// <summary>
            /// Get all posts for admin (Admin only)
            /// </summary>
            [HttpGet("admin/posts")]
            [Authorize(Roles = "Admin")]
            public async Task<ActionResult<PaginatedResultDto<BlogPostSummaryDto>>> GetAllPostsAdmin([FromQuery] BlogFilterDto filter)
            {
                try
                {
                    // Admin can see all posts regardless of status
                    var posts = await _blogService.GetPostsAsync(filter);
                    return Ok(posts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving all posts for admin");
                    return StatusCode(500, "Internal server error");
                }
            }

        }
    }

