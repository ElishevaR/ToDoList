using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB")));
});

var app = builder.Build();
app.UseCors(policy =>
{
    policy.AllowAnyOrigin(); // מרשה גישה מכל מקור
    policy.AllowAnyMethod(); // מרשה כל פעולה (GET, POST, PUT, DELETE וכו')
    policy.AllowAnyHeader(); // מרשה כל כותרת בבקשה
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});

app.MapGet("/", () => "Hello World!");

// Get all items
app.MapGet("/items", async context =>
{
    using var db = context.RequestServices.GetRequiredService<ToDoDbContext>();
    var items = await db.Items.ToListAsync();
    await context.Response.WriteAsJsonAsync(items);
});
app.MapPost("/items", async (ToDoDbContext dbContext, HttpRequest request) =>
{
    var taskData = await request.ReadFromJsonAsync<Item>(); // לקרוא את נתוני המשימה החדשה מהבקשה
    var newTask = new Item { Name = taskData.Name }; // ליצור רשומת משימה חדשה
    dbContext.Items.Add(newTask); // להוסיף את המשימה החדשה למסד הנתונים
    await dbContext.SaveChangesAsync(); // לשמור את השינויים במסד הנתונים
    return Results.Created($"/tasks/{newTask.Id}", newTask); // להחזיר תשובת הצלחה עם מידע על המשימה החדשה
});
app.MapPut("/items/{id}", async (ToDoDbContext dbContext, int id, HttpRequest request) =>
{
    var taskData = await request.ReadFromJsonAsync<Item>(); // לקרוא את נתוני המשימה המעודכנים מהבקשה
    var existingTask = await dbContext.Items.FindAsync(id); // למצוא את המשימה הקיימת במסד הנתונים על פי המזהה

    if (existingTask == null) // בדיקה אם המשימה לא נמצאה
    {
        return Results.NotFound(); // להחזיר תשובת טעות עם קוד 404 - לא נמצא
    }
 existingTask.Name = taskData.Name;// לעדכן את השדה של שם המשימה
    existingTask.IsComplete = taskData.IsComplete; // לעדכן את השדה של הסטטוס של המשימה

    await dbContext.SaveChangesAsync(); // לשמור את השינויים במסד הנתונים

    return Results.Ok(existingTask); // להחזיר תשובת הצלחה עם מידע על המשימה המעודכנת
});
app.MapDelete("/items/{id}", async (int id, ToDoDbContext dbContext) =>
{
    var taskToRemove = await dbContext.Items.FindAsync(id); // למצוא את המשימה שיש למחוק לפי המזהה
    if (taskToRemove == null) return Results.NotFound(); // אם המשימה לא נמצאה, להחזיר שגיאת דרישה לא נמצא

    dbContext.Items.Remove(taskToRemove); // להסיר את המשימה ממסד הנתונים
    await dbContext.SaveChangesAsync(); // לשמור את השינויים במסד הנתונים
    return Results.Ok(); // להחזיר תשובת הצלחה
});

app.Run();