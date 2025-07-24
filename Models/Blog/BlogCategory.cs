using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.Models.Blog
{
    public class BlogCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }  // Optional

        [StringLength(100)]
        public string? Slug { get; set; }         // Optional

        public bool IsActive { get; set; }       // Optional

        public DateTime? CreatedAt { get; set; }  // Optional

        public DateTime? UpdatedAt { get; set; }  // Optional

        // Navigation property
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}
