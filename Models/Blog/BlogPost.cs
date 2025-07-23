using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MentalHealthApis.Models.Blog
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        // FIX: Add '?' to make this property nullable
        [StringLength(500)]
        public string? Summary { get; set; }

        [Required]
        [StringLength(200)]
        public string Slug { get; set; }

        // FIX: Add '?' to make this property nullable (already done in previous step)
        [StringLength(255)]
        public string FeaturedImage { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public PostStatus Status { get; set; } = PostStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;

        // FIX: Add '?' to make this property nullable
        [StringLength(500)]
        public string? MetaDescription { get; set; }

        // FIX: Add '?' to make this property nullable
        [StringLength(200)]
        public string? MetaKeywords { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual BlogCategory Category { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }

        public virtual ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();
        public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
    }
}