using MentalHealthApis.Models.Blog;
using System.ComponentModel.DataAnnotations;
using MentalHealthApis.Models;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace MentalHealthApis.DTOs.Blog
{
    public class BlogPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string Slug { get; set; }
        public string FeaturedImage { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class BlogPostSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Slug { get; set; }
        public string FeaturedImage { get; set; }
        public string CategoryName { get; set; }
        public string AuthorName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class CreateBlogPostDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(500)]
        public string? Summary { get; set; }

        // This property will receive the uploaded file from the form.
        // The name "FeaturedImageFile" should be used in the service layer.
        public IFormFile? FeaturedImageFile { get; set; }
        
        // This will hold the URL/path of the image after it's saved.
        // The service will populate this before saving to the database.
        public string? FeaturedImage { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public PostStatus Status { get; set; } = PostStatus.Draft;

        public bool IsFeatured { get; set; } = false;

        [StringLength(500)]
        public string? MetaDescription { get; set; }

        [StringLength(200)]
        public string? MetaKeywords { get; set; }

        // Changed to a single string to accept comma-separated values from the form.
        public string? Tags { get; set; }
    }

    public class UpdateBlogPostDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(500)]
        public string? Summary { get; set; }
        
        // This property will receive a new uploaded file, if any.
        public IFormFile? FeaturedImageFile { get; set; }

        [StringLength(255)]
        public string? FeaturedImage { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public PostStatus Status { get; set; }

        public bool IsFeatured { get; set; }

        [StringLength(500)]
        public string? MetaDescription { get; set; }

        [StringLength(200)]
        public string? MetaKeywords { get; set; }
        
        // Changed to a single string to accept comma-separated values from the form.
        public string? Tags { get; set; }
    }
}