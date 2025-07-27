using Microsoft.EntityFrameworkCore;
using MentalHealthApis.Data;
using MentalHealthApis.DTOs.Blog;
using MentalHealthApis.Models;
using MentalHealthApis.Models.Blog;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogService> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IJournalEntriesService _journalService;

        public BlogService(
            ApplicationDbContext context,
            ILogger<BlogService> logger,
            IWebHostEnvironment hostingEnvironment,
            IJournalEntriesService journalService)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _journalService = journalService;
        }

        #region Existing Methods
        // ... All your other methods like GetCategoriesAsync, CreatePostAsync, etc. are correct and remain here ...
        public async Task<List<BlogCategoryDto>> GetCategoriesAsync()
        {
            return await _context.BlogCategories
    .Where(c => (bool)c.IsActive)
    .OrderBy(c => c.Name)
    .Select(c => new BlogCategoryDto
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        Slug = c.Slug,
        IsActive = (bool)c.IsActive,
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
                    IsActive = (bool)c.IsActive,
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
            if (hasPosts) return false; 

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BlogPostSummaryDto>> GetAllPublishedPostsAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
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

    string? featuredImagePath = null; 

    if (dto.FeaturedImageFile != null && dto.FeaturedImageFile.Length > 0)
    {
        _logger.LogInformation("FeaturedImageFile is present. Saving file...");
        featuredImagePath = await SaveFile(dto.FeaturedImageFile);
        _logger.LogInformation("File saved. Path: {FilePath}", featuredImagePath);
    }
    else
    {
        _logger.LogWarning("No FeaturedImageFile was uploaded.");
    }

    var post = new BlogPost
    {
        Title = dto.Title,
        Content = dto.Content,
        Summary = dto.Summary,
        Slug = GenerateSlug(dto.Title),
        FeaturedImage = featuredImagePath,
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
    
    _logger.LogInformation("Attempting to save new BlogPost to database...");

    await _context.SaveChangesAsync();
    
    _logger.LogInformation("BlogPost saved successfully with ID: {PostId}", post.Id);

    if (!string.IsNullOrWhiteSpace(dto.Tags))
    {
        var tagNames = dto.Tags.Split(',').Select(t => t.Trim()).ToList();
        await UpdatePostTagsAsync(post.Id, tagNames);
    }

    var result = await GetPostByIdAsync(post.Id);
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

            if (dto.FeaturedImageFile != null)
            {
                post.FeaturedImage = await SaveFile(dto.FeaturedImageFile);
            }

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.Summary = dto.Summary;
            post.Slug = GenerateSlug(dto.Title);
            post.CategoryId = dto.CategoryId;
            post.Status = dto.Status;
            post.IsFeatured = dto.IsFeatured;
            post.MetaDescription = dto.MetaDescription;
            post.MetaKeywords = dto.MetaKeywords;
            post.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == PostStatus.Published && post.PublishedAt == null)
                post.PublishedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrWhiteSpace(dto.Tags))
            {
                var tagNames = dto.Tags.Split(',').Select(t => t.Trim()).ToList();
                await UpdatePostTagsAsync(post.Id, tagNames);
            } 
            else 
            {
                post.Tags.Clear();
            }

            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(post.Id);
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

        public async Task<List<string>> GetTagsAsync()
        {
            return await _context.BlogTags
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<BlogPostSummaryDto>> GetPostsByTagAsync(string tag)
        {
            var normalizedTag = tag.ToLowerInvariant();

            var query = _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Tags)
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
        
        private async Task<string> SaveFile(IFormFile file)
        {
            var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "blog");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/blog/{uniqueFileName}";
        }

        private async Task UpdatePostTagsAsync(int postId, IEnumerable<string> tagNames)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return;

            post.Tags.Clear();

            foreach (var name in tagNames.Where(n => !string.IsNullOrEmpty(n)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var tag = await _context.BlogTags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower())
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
                 .Replace(".", "")
                 .Replace("?", "")
                 .Replace("!", "")
                 .Replace(":", "")
                 .Replace(";", "");

        private async Task<bool> IsAdminAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Role == UserRole.Admin;
        }
        #endregion

        #region Recommendations
        // =================================================================
        // =========== THIS IS THE NEW, CORRECTED METHOD ============
        // =================================================================
        public async Task<List<BlogPostSummaryDto>> GetRecommendedPostsForUserAsync(int userId)
        {
            // Step 1: Get the user's latest sentiment
            var latestSentiment = await _journalService.GetLatestSentimentForUserAsync(userId);

            if (latestSentiment == null)
            {
                // Return an empty list if there's no journal history
                return new List<BlogPostSummaryDto>();
            }

            // Step 2: Convert the boolean sentiments into a list of search tags
            var tagsToSearch = new List<string>();
            if (latestSentiment.Anxiety) tagsToSearch.Add("anxiety");
            if (latestSentiment.Depression) tagsToSearch.Add("depression");
            if (latestSentiment.Stress) tagsToSearch.Add("stress");
            if (latestSentiment.Bipolar) tagsToSearch.Add("bipolar");
            if (latestSentiment.Suicidal) tagsToSearch.Add("suicide");
            if (latestSentiment.PersonalityDisorder) tagsToSearch.Add("personality-disorder");

            // If no negative sentiments, recommend general wellness topics
            if (!tagsToSearch.Any() && latestSentiment.Normal)
            {
                tagsToSearch.Add("wellness");
                tagsToSearch.Add("mindfulness");
            }

            if (!tagsToSearch.Any())
            {
                // No relevant sentiments were detected
                return new List<BlogPostSummaryDto>();
            }

            _logger.LogInformation("For user {UserId}, searching for posts with tags: {Tags}", userId, string.Join(", ", tagsToSearch));

            // Step 3: Directly query for blog posts matching the tags
            var recommendedPosts = await _context.BlogPosts
                // Filter for posts that are published and in an active category
                .Where(post => post.Status == PostStatus.Published &&
                               post.Category.IsActive == true &&
                               // The post must have at least one tag that is in our search list
                               post.Tags.Any(tag => tagsToSearch.Contains(tag.Slug)))
                .OrderByDescending(post => post.PublishedAt)
                // Project the database result into our desired DTO format
                .Select(post => new BlogPostSummaryDto
                {
                    Id = post.Id,
                    Title = post.Title,
                    Summary = post.Summary,
                    Slug = post.Slug,
                    FeaturedImage = post.FeaturedImage,
                    CategoryName = post.Category.Name,
                    AuthorName = post.Author.Name,
                    PublishedAt = post.PublishedAt,
                    ViewCount = post.ViewCount,
                    IsFeatured = post.IsFeatured,
                    Tags = post.Tags.Select(t => t.Name).ToList()
                })
                .Distinct() // Ensures a post isn't recommended twice if it has multiple matching tags
                .Take(20)   // Limit the number of recommendations
                .ToListAsync();

            return recommendedPosts;
        }

        #endregion
    }
}