﻿using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.DTOs.Blog
{
    public class BlogCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } // returned from DB
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public bool IsActive { get; set; }
        public int PostCount { get; set; }
    }

    public class CreateBlogCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateBlogCategoryDto
    {
        [StringLength(100)]
        public string? Name { get; set; } // Optional for update

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
