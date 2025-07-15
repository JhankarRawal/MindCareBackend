using MentalHealthApis.Models;

namespace MentalHealthApis.DTOs.Blog
{
    public class BlogFilterDto
    {
        public int? CategoryId { get; set; }
        public string CategorySlug { get; set; }
        public string Tag { get; set; }
        public string Search { get; set; }
        public PostStatus? Status { get; set; }
        public bool? IsFeatured { get; set; }
        public int? AuthorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "PublishedAt";
        public string SortOrder { get; set; } = "desc";
    }
}
