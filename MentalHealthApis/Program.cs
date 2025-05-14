using MentalHealthApis.Data;
using MentalHealthApis.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpContextAccessor for accessing HttpContext in services if needed
builder.Services.AddHttpContextAccessor();

// Register custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>(); // Added
builder.Services.AddScoped<IDoctorService, DoctorService>(); // Added
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Configure controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings in JSON responses
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // You might also want to ignore null values to keep JSON responses cleaner
        // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 2. Configure Authentication & Authorization

// Add JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // Often good to set DefaultScheme
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Add Authorization services
builder.Services.AddAuthorization(); // This enables attribute-based authorization like [Authorize]

// 3. Configure Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer(); // Required for minimal APIs and Swagger
builder.Services.AddSwaggerGen(options => // Renamed 'c' to 'options' for clarity
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Mental Health API", Version = "v1" });

    // Define the BearerAuth security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey, // Using ApiKey for Bearer token in header
        Scheme = "Bearer"
    });

    // Make sure Swagger UI requires a Bearer token to authorize requests
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Must match the name given in AddSecurityDefinition
                },
                Scheme = "oauth2", // Not strictly necessary but common
                Name = "Bearer",   // Must match
                In = ParameterLocation.Header,
            },
            new List<string>() // No specific scopes for now
        }
    });
});

// 4. Configure CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", // Define a policy name
        policyBuilder => // Renamed 'builder' to 'policyBuilder' to avoid conflict with WebApplicationBuilder
        {
            policyBuilder.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[] { "http://localhost:3000" }) // Load from config or default
                         .AllowAnyHeader()
                         .AllowAnyMethod();
            // For development, you might allow any origin if needed:
            // .AllowAnyOrigin();
        });
});


// --- Build the application ---
var app = builder.Build();

// 5. Configure the HTTP request pipeline (middleware). Order is important.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => // Configure Swagger UI endpoint
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mental Health API V1");
        // options.RoutePrefix = string.Empty; // To serve Swagger UI at app's root
    });

    // Apply database migrations automatically during development
    // For production, consider a more robust migration strategy (e.g., separate deployment step)
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        // Log the error or handle it appropriately
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        // Optionally, rethrow or handle to prevent app startup if DB is critical
    }
    // Add developer exception page for better error diagnostics in development
    app.UseDeveloperExceptionPage();
}
else
{
    // For production, use a generic error handler
    app.UseExceptionHandler("/Error"); // You would need to create an Error handling endpoint or page
    app.UseHsts(); // Adds HTTP Strict Transport Security Protocol header
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

app.UseCors("AllowSpecificOrigin"); // Enable CORS policy

app.UseAuthentication(); // !!! Must be called BEFORE UseAuthorization !!!
app.UseAuthorization();  // Enables authorization checks for controllers/endpoints

app.MapControllers(); // Maps attribute-routed controllers

app.Run();