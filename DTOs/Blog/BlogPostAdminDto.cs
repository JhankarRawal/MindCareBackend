
using MentalHealthApis.Models;
public class BlogPostAdminDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ContentPreview { get; set; }
    public PostStatus Status { get; set; } // e.g., "Pending", "Approved", "Rejected"
    public int AuthorId { get; set; }
}
