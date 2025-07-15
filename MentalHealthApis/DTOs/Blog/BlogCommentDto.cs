using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.DTOs.Blog
{
    public class BlogCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public int? AuthorId { get; set; }
        public int BlogPostId { get; set; }
        public int? ParentCommentId { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BlogCommentDto> Replies { get; set; } = new List<BlogCommentDto>();
    }

    public class CreateBlogCommentDto
    {
        [Required]
        public string Content { get; set; }

        [Required]
        public int BlogPostId { get; set; }

        public int? ParentCommentId { get; set; }
    }

    public class UpdateBlogCommentDto
    {
        [Required]
        public string Content { get; set; }
    }
}
