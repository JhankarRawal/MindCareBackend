using Microsoft.EntityFrameworkCore;

using MentalHealthApis.Models.Blog;

namespace MentalHealthApis.Data
{
    public static class BlogSeeder
    {
        public static async Task SeedBlogDataAsync(ApplicationDbContext context)
        {
            // Seed Categories
            if (!await context.BlogCategories.AnyAsync())
            {
                var categories = new List<BlogCategory>
                {
                    new BlogCategory
                    {
                        Name = "Stress Management",
                        Description = "Tips, techniques, and strategies for managing stress in daily life",
                        Slug = "stress-management",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Bipolar Disorder",
                        Description = "Understanding, coping strategies, and support for bipolar disorder",
                        Slug = "bipolar-disorder",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Depression",
                        Description = "Resources, support, and information about depression",
                        Slug = "depression",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Anxiety Disorders",
                        Description = "Managing anxiety, panic attacks, and related conditions",
                        Slug = "anxiety-disorders",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Personality Disorders",
                        Description = "Understanding and living with personality disorders",
                        Slug = "personality-disorders",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Self-Care & Wellness",
                        Description = "Mental health self-care practices and wellness tips",
                        Slug = "self-care-wellness",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Therapy & Treatment",
                        Description = "Different therapy approaches, treatments, and professional help",
                        Slug = "therapy-treatment",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Relationships & Social Support",
                        Description = "Building healthy relationships and finding social support",
                        Slug = "relationships-social-support",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Trauma & PTSD",
                        Description = "Understanding trauma, PTSD, and recovery strategies",
                        Slug = "trauma-ptsd",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BlogCategory
                    {
                        Name = "Mindfulness & Meditation",
                        Description = "Mindfulness practices, meditation, and mental clarity",
                        Slug = "mindfulness-meditation",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.BlogCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Seed Tags
            if (!await context.BlogTags.AnyAsync())
            {
                var tags = new List<BlogTag>
                {
                    new BlogTag { Name = "Mental Health", Slug = "mental-health", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Coping Strategies", Slug = "coping-strategies", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Therapy", Slug = "therapy", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Wellness", Slug = "wellness", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Self-Help", Slug = "self-help", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Support", Slug = "support", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Recovery", Slug = "recovery", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Awareness", Slug = "awareness", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Professional Help", Slug = "professional-help", CreatedAt = DateTime.UtcNow },
                    new BlogTag { Name = "Daily Life", Slug = "daily-life", CreatedAt = DateTime.UtcNow }
                };

                context.BlogTags.AddRange(tags);
                await context.SaveChangesAsync();
            }
        }
    }
}
