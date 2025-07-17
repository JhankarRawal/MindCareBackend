using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealthApis.Models.Blog
{
    public class BlogComment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int? AuthorId { get; set; }

        [Required]
        public int BlogPostId { get; set; }

        public int? ParentCommentId { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        //[ForeignKey("AuthorId")]
        //public virtual ApplicationUser Author { get; set; }

        [ForeignKey("BlogPostId")]
        public virtual BlogPost BlogPost { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual BlogComment ParentComment { get; set; }

        public virtual ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    }
}