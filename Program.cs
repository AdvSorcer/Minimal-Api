using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CmsDatabaseContext>(option => option.UseInMemoryDatabase("CmsDatabse"));
builder.Services.AddAutoMapper(typeof(CmsMapper));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//Get
app.MapGet("/courses", async (CmsDatabaseContext db) =>
{
    var result = await db.Courses.ToListAsync();
    return Results.Ok(result);
});

//Post
app.MapPost("/courses", async (CourseDto courseDto, CmsDatabaseContext db, IMapper mapper) =>
{
    try
    {
        var newCourse = mapper.Map<Course>(courseDto);
        db.Courses.Add(newCourse);
        await db.SaveChangesAsync();
        var result = mapper.Map<CourseDto>(newCourse);

        return Results.Created($"/courses/{result.CourseId}", result);
    }
    catch (System.Exception ex)
    {

        // throw new InvalidOperationExcetion();
        // return Results.StatusCode(500);
        return Results.Problem(ex.Message);
    }

});

//GetSingleItem
app.MapGet("/courses/{courseId}", async (int courseId, CmsDatabaseContext db, IMapper mapper) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if (course == null)
    {
        return Results.NotFound();
    }
    var result = mapper.Map<CourseDto>(course);
    return Results.Ok(result);
});

//UpdateSingleItem
app.MapPut("/courses/{courseId}", async (int courseId, CourseDto courseDto, CmsDatabaseContext db, IMapper mapper) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if (course == null)
    {
        return Results.NotFound();
    }
    course.CourseName = courseDto.CourseName;
    course.CourseDuration = courseDto.CourseDuration;
    course.CourseType = (int)courseDto.CourseType;
    await db.SaveChangesAsync();

    var result = mapper.Map<CourseDto>(course);
    return Results.Ok(result);
});

//DeleteSingleItem
app.MapDelete("/courses/{courseId}", async (int courseId, CmsDatabaseContext db, IMapper mapper) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if (course == null)
    {
        return Results.NotFound();
    }
    db.Courses.Remove(course);
    await db.SaveChangesAsync();

    var result = mapper.Map<CourseDto>(course);
    return Results.Ok(result);
});


app.Run();

public class CmsMapper : Profile
{
    public CmsMapper()
    {
        CreateMap<Course, CourseDto>();
        CreateMap<CourseDto, Course>();
    }
}

public class Course
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int CourseDuration { get; set; }
    public int CourseType { get; set; }
}

public class CourseDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int CourseDuration { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public COURSE_TYPE CourseType { get; set; }
}

public enum COURSE_TYPE
{
    ENGINEERING = 1,
    MEDICAL,
    MANAGEMENT
}

public class CmsDatabaseContext : DbContext
{
    public DbSet<Course> Courses => Set<Course>();
    public CmsDatabaseContext(DbContextOptions options) : base(options)
    {
    }

}