using Microsoft.EntityFrameworkCore;
using MentalHealthApis.Data;
using MentalHealthApis.DTOs.Blog;
using MentalHealthApis.Models;
using MentalHealthApis.Models.Blog;

namespace MentalHealthApis.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogService> _logger;

        public BlogService(ApplicationDbContext context, ILogger<BlogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /* ───────────────────────────────
           CATEGORY QUERIES
           ─────────────────────────────── */

        public async Task<List<BlogCategoryDto>> GetCategoriesAsync()
        {
            return await _context.BlogCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new BlogCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    IsActive = c.IsActive,
                    PostCount = c.BlogPosts.Count(p => p.Status == PostStatus.Published)
                })
                .ToListAsync();
        }

        public async Task<BlogCategoryDto?> GetCategoryByIdAsync(int id)
        {
            return await _context.BlogCategories
                .Where(c => c.Id == id)
                .Select(c => new BlogCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    IsActive = c.IsActive,
                    PostCount = c.BlogPosts.Count(p => p.Status == PostStatus.Published)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug)
        {
            return await _context.BlogCategories
                .Where(c => c.Slug == slug)
                .Select(c => new BlogCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    IsActive = c.IsActive,
                    PostCount = c.BlogPosts.Count(p => p.Status == PostStatus.Published)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<BlogCategoryDto> CreateCategoryAsync(CreateBlogCategoryDto dto)
        {
            var category = new BlogCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = GenerateSlug(dto.Name),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogCategories.Add(category);
            await _context.SaveChangesAsync();

            return new BlogCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                IsActive = category.IsActive,
                PostCount = 0
            };
        }

        public async Task<BlogCategoryDto?> UpdateCategoryAsync(int id, UpdateBlogCategoryDto dto)
        {
            var category = await _context.BlogCategories.FindAsync(id);
            if (category is null) return null;

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Slug = GenerateSlug(dto.Name);
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new BlogCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                IsActive = category.IsActive,
                PostCount = await _context.BlogPosts.CountAsync(p =>
                                p.CategoryId == id && p.Status == PostStatus.Published)
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.BlogCategories.FindAsync(id);
            if (category is null) return false;

            bool hasPosts = await _context.BlogPosts.AnyAsync(p => p.CategoryId == id);
            if (hasPosts) return false; // guard: cannot delete with posts

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        /* ───────────────────────────────
           POST QUERIES
           ─────────────────────────────── */

        public async Task<PaginatedResultDto<BlogPostSummaryDto>> GetPostsAsync(BlogFilterDto filter)
        {
            var query = _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .AsQueryable();

            /* filters */
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
                query = query.Where(p => p.Category.Slug == filter.CategorySlug);

            if (!string.IsNullOrWhiteSpace(filter.Tag))
                query = query.Where(p => p.Tags.Any(t => t.Slug == filter.Tag));

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(p => p.Title.Contains(filter.Search) || p.Content.Contains(filter.Search));

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            if (filter.AuthorId.HasValue)
                query = query.Where(p => p.AuthorId == filter.AuthorId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.ToDate.Value);

            /* sorting */
            query = filter.SortBy.ToLower() switch
            {
                "title" => filter.SortOrder == "asc"
                                ? query.OrderBy(p => p.Title)
                                : query.OrderByDescending(p => p.Title),
                "createdat" => filter.SortOrder == "asc"
                                ? query.OrderBy(p => p.CreatedAt)
                                : query.OrderByDescending(p => p.CreatedAt),
                "viewcount" => filter.SortOrder == "asc"
                                ? query.OrderBy(p => p.ViewCount)
                                : query.OrderByDescending(p => p.ViewCount),
                _ => filter.SortOrder == "asc"
                                ? query.OrderBy(p => p.PublishedAt)
                                : query.OrderByDescending(p => p.PublishedAt)
            };

            /* pagination */
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new BlogPostSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Summary = p.Summary,
                    Slug = p.Slug,
                    FeaturedImage = p.FeaturedImage,
                    CategoryName = p.Category.Name,
                    AuthorName = p.Author.Name,
                    PublishedAt = p.PublishedAt,
                    ViewCount = p.ViewCount,
                    IsFeatured = p.IsFeatured,
                    Tags = p.Tags.Select(t => t.Name).ToList(),
                    CommentCount = p.Comments.Count(c => c.IsApproved)
                })
                .ToListAsync();

            return new PaginatedResultDto<BlogPostSummaryDto>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = filter.Page > 1,
                HasNextPage = filter.Page < totalPages
            };
        }

        public async Task<BlogPostDto?> GetPostByIdAsync(int id)
        {
            return await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.Id == id)
                .Select(p => MapToDto(p))
                .FirstOrDefaultAsync();
        }

        public async Task<BlogPostDto?> GetPostBySlugAsync(string slug)
        {
            return await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.Slug == slug)
                .Select(p => MapToDto(p))
                .FirstOrDefaultAsync();
        }

        public async Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto, string authorId)
        {
            if (!int.TryParse(authorId, out int parsedAuthorId))
                throw new ArgumentException("Invalid author ID format.");

            var post = new BlogPost
            {
                Title = dto.Title,
                Content = dto.Content,
                Summary = dto.Summary,
                Slug = GenerateSlug(dto.Title),
                FeaturedImage = dto.FeaturedImage,
                CategoryId = dto.CategoryId,
                AuthorId = parsedAuthorId,
                Status = dto.Status,
                IsFeatured = dto.IsFeatured,
                MetaDescription = dto.MetaDescription,
                MetaKeywords = dto.MetaKeywords,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = dto.Status == PostStatus.Published ? DateTime.UtcNow : null
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            if (dto.Tags?.Count > 0)
                await UpdatePostTagsAsync(post.Id, dto.Tags);

            var result = await GetPostByIdAsync(post.Id);
            if (result == null)
                throw new Exception("Post creation succeeded but retrieval failed.");

            return result;
        }



        public async Task<BlogPostDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto,string authorId)
        {
                var post = await _context.BlogPosts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);
                 if (!int.TryParse(authorId, out int parsedAuthorId))


                if (post is null) return null;
            if (post.AuthorId != parsedAuthorId && !await IsAdminAsync(parsedAuthorId))
                throw new UnauthorizedAccessException("Not authorised");

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.Summary = dto.Summary;
            post.Slug = GenerateSlug(dto.Title);
            post.FeaturedImage = dto.FeaturedImage;
            post.CategoryId = dto.CategoryId;
            post.Status = dto.Status;
            post.IsFeatured = dto.IsFeatured;
            post.MetaDescription = dto.MetaDescription;
            post.MetaKeywords = dto.MetaKeywords;
            post.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == PostStatus.Published && post.PublishedAt is null)
                post.PublishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (dto.Tags?.Count > 0)
                await UpdatePostTagsAsync(post.Id, dto.Tags);

            return await GetPostByIdAsync(post.Id)!;
        }

        public async Task<bool> DeletePostAsync(int id, string authorId)
        {

            var post = await _context.BlogPosts.FindAsync(id);
            if (!int.TryParse(authorId, out int parsedAuthorId))

            if (post is null) return false;
            if (post.AuthorId != parsedAuthorId && !await IsAdminAsync(parsedAuthorId))
                throw new UnauthorizedAccessException("Not authorised");

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishPostAsync(int id, string authorId)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (!int.TryParse(authorId, out int parsedAuthorId))

                if (post is null) return false;
            if (post.AuthorId != parsedAuthorId && !await IsAdminAsync(parsedAuthorId))
                throw new UnauthorizedAccessException("Not authorised");

            post.Status = PostStatus.Published;
            post.PublishedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post is null) return false;

            post.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        /* ───────────────────────────────
           TAG & COMMENT helpers (stubs)
           ─────────────────────────────── */
        private BlogCommentDto MapComment(BlogComment c) => new()
        {
            Id = c.Id,
            Content = c.Content,
            AuthorId = c.AuthorId,
            AuthorName = _context.Users
                      .Where(u => u.Id == c.AuthorId)
                      .Select(u => u.Name)
                      .FirstOrDefault(),
            BlogPostId = c.BlogPostId,
            ParentCommentId = c.ParentCommentId,
            IsApproved = c.IsApproved,
            CreatedAt = c.CreatedAt,
            Replies = c.Replies
                    .Where(r => r.IsApproved)
                    .OrderBy(r => r.CreatedAt)
                    .Select(MapComment)
                    .ToList()
        };

        public async Task<List<BlogCommentDto>> GetPostCommentsAsync(int postId)
        {
            return await _context.BlogComments
                .Where(c => c.BlogPostId == postId && c.ParentCommentId == null && c.IsApproved)
                .Include(c => c.Replies.Where(r => r.IsApproved))
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => MapComment(c))
                .ToListAsync();
        }
        public async Task<BlogCommentDto> CreateCommentAsync(CreateBlogCommentDto dto, string authorId)
        {
            if (!int.TryParse(authorId, out int parsedAuthorId))
                throw new ArgumentException("Invalid author ID format.");
            var comment = new BlogComment
            {
                Content = dto.Content,
                AuthorId = parsedAuthorId,
                BlogPostId = dto.BlogPostId,
                ParentCommentId = dto.ParentCommentId,
                IsApproved = false,               // moderation required
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogComments.Add(comment);
            await _context.SaveChangesAsync();
            return MapComment(await _context.BlogComments
                                .Include(c => c.Replies)
                                .FirstAsync(c => c.Id == comment.Id));
        }
        public async Task<BlogCommentDto> UpdateCommentAsync(int id,UpdateBlogCommentDto dto, string authorId)
        {
            var comment = await _context.BlogComments.FindAsync(id);
            if (!int.TryParse(authorId, out int parsedAuthorId))
                throw new ArgumentException("Invalid author ID format.");
            if (comment is null) throw new KeyNotFoundException("Comment not found");

            if (comment.AuthorId != parsedAuthorId && !await IsAdminAsync(parsedAuthorId))
                throw new UnauthorizedAccessException("Not authorised");

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapComment(comment);
        }
        public async Task<bool> DeleteCommentAsync(int id, string authorId)
        {
            var comment = await _context.BlogComments
                                        .Include(c => c.Replies)
                                        .FirstOrDefaultAsync(c => c.Id == id);
            if (comment is null) return false;
            if (!int.TryParse(authorId, out int parsedAuthorId))
                throw new ArgumentException("Invalid author ID format.");

            if (comment.AuthorId != parsedAuthorId && !await IsAdminAsync(parsedAuthorId))
                throw new UnauthorizedAccessException("Not authorised");

            // remove child replies too
            _context.BlogComments.RemoveRange(comment.Replies);
            _context.BlogComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ApproveCommentAsync(int id)
        {
            var comment = await _context.BlogComments.FindAsync(id);
            if (comment is null) return false;

            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<string>> GetTagsAsync()
        {
            return await _context.BlogTags
                                 .OrderBy(t => t.Name)
                                 .Select(t => t.Name)
                                 .ToListAsync();
        }
        public async Task<List<BlogPostSummaryDto>> GetPostsByTagAsync(string tag,
                                                                       int page = 1,
                                                                       int size = 10)
        {
            var query = _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.Status == PostStatus.Published &&
                            p.Tags.Any(t => t.Slug == tag.ToLower()))
                .OrderByDescending(p => p.PublishedAt);

            return await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new BlogPostSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Summary = p.Summary,
                    Slug = p.Slug,
                    FeaturedImage = p.FeaturedImage,
                    CategoryName = p.Category.Name,
                    AuthorName = p.Author.Name,
                    PublishedAt = p.PublishedAt,
                    ViewCount = p.ViewCount,
                    IsFeatured = p.IsFeatured,
                    Tags = p.Tags.Select(t => t.Name).ToList(),
                    CommentCount = p.Comments.Count(c => c.IsApproved)
                })
                .ToListAsync();
        }
        /* ───────────────────────────────
           PRIVATE HELPERS
           ─────────────────────────────── */

        private async Task UpdatePostTagsAsync(int postId, IEnumerable<string> tagNames)

        {
            var post = await _context.BlogPosts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (post is null) return;

            post.Tags.Clear();

            foreach (var name in tagNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var tag = await _context.BlogTags.FirstOrDefaultAsync(t => t.Name == name)
                          ?? _context.BlogTags.Add(new BlogTag
                          {
                              Name = name,
                              Slug = GenerateSlug(name),
                              CreatedAt = DateTime.UtcNow
                          }).Entity;
                post.Tags.Add(tag);
            }

            await _context.SaveChangesAsync();
        }

        private static string GenerateSlug(string title) =>
            title.ToLowerInvariant()
                 .Replace(" ", "-")
                 .Replace("'", "")
                 .Replace("\"", "")
                 .Replace(",", "")
                 .Replace(".", "")
                 .Replace("!", "")
                 .Replace("?", "")
                 .Replace(":", "")
                 .Replace(";", "")
                 .Replace("&", "and")
                 .Replace("@", "at");

        private static BlogPostDto MapToDto(BlogPost p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Summary = p.Summary,
            Slug = p.Slug,
            FeaturedImage = p.FeaturedImage,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            AuthorId = p.AuthorId,
            AuthorName = p.Author.Name,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            PublishedAt = p.PublishedAt,
            ViewCount = p.ViewCount,
            IsFeatured = p.IsFeatured,
            MetaDescription = p.MetaDescription,
            MetaKeywords = p.MetaKeywords,
            Tags = p.Tags.Select(t => t.Name).ToList(),
            CommentCount = p.Comments.Count(c => c.IsApproved)
        };

        private async Task<bool> IsAdminAsync(int userId) =>
            (await _context.Users.FindAsync(userId))?.Role == UserRole.Admin;

    }
}
