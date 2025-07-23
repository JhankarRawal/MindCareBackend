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

        #region Categories

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
            if (category == null) return null;

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
                PostCount = await _context.BlogPosts.CountAsync(p => p.CategoryId == id && p.Status == PostStatus.Published)
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.BlogCategories.FindAsync(id);
            if (category == null) return false;

            bool hasPosts = await _context.BlogPosts.AnyAsync(p => p.CategoryId == id);
            if (hasPosts) return false; // Guard: cannot delete category with posts

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Posts

        public async Task<List<BlogPostSummaryDto>> GetAllPublishedPostsAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.PublishedAt)
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
                })
                .ToListAsync();
        }

        public async Task<List<BlogPostSummaryDto>> GetPostsByCategorySlugAsync(string categorySlug)
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published && p.Category.Slug == categorySlug)
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.PublishedAt)
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
                })
                .ToListAsync();
        }
        public async Task<BlogPostDto?> GetPostByIdAsync(int id)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.Id == id)
                .Select(p => new BlogPostDto
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
                })
                .FirstOrDefaultAsync();

            return post;
        }




        public async Task<BlogPostDto?> GetPostBySlugAsync(string slug)
        {
            return await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.Slug == slug)
                .Select(p => new BlogPostDto
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
                })
                .FirstOrDefaultAsync();
        }


        public async Task<List<BlogPostSummaryDto>> GetAllPostsAsync()
        {
            var posts = await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.PublishedAt)
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
                })
                .ToListAsync();

            return posts;
        }

        public async Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto, string authorId)
        {
            if (!int.TryParse(authorId, out var parsedAuthorId))
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

            var result = await GetPostBySlugAsync(post.Slug);
            if (result == null)
                throw new Exception("Post creation succeeded but retrieval failed.");

            return result;
        }

        public async Task<BlogPostDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto, string authorId)
        {
            if (!int.TryParse(authorId, out var parsedAuthorId))
                throw new ArgumentException("Invalid author ID format.");

            var post = await _context.BlogPosts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return null;

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

            if (dto.Status == PostStatus.Published && post.PublishedAt == null)
                post.PublishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (dto.Tags?.Count > 0)
                await UpdatePostTagsAsync(post.Id, dto.Tags);

            return await GetPostBySlugAsync(post.Slug);
        }

        public async Task<bool> DeletePostAsync(int id, string authorId)
        {
            if (!int.TryParse(authorId, out var parsedAuthorId))
                throw new ArgumentException("Invalid author ID format.");

            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

            if (post.AuthorId != parsedAuthorId && !await IsAdminAsync(parsedAuthorId))
                throw new UnauthorizedAccessException("Not authorised");

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishPostAsync(int id, string authorId)
        {
            if (!int.TryParse(authorId, out var parsedAuthorId))
                throw new ArgumentException("Invalid author ID format.");

            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

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
            if (post == null) return false;

            post.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }


        #endregion

        #region Tags

        public async Task<List<string>> GetTagsAsync()
        {
            return await _context.BlogTags
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<BlogPostSummaryDto>> GetPostsByTagAsync(string tag)
        {
            var normalizedTag = tag.ToLowerInvariant();

            var query = _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.Status == PostStatus.Published && p.Tags.Any(t => t.Slug == normalizedTag))
                .OrderByDescending(p => p.PublishedAt);

            return await query
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
                })
                .ToListAsync();
        }


        #endregion

        #region Helpers

        private async Task UpdatePostTagsAsync(int postId, IEnumerable<string> tagNames)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return;

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
        };

        private async Task<bool> IsAdminAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Role == UserRole.Admin;
        }

        #endregion
    }
}
