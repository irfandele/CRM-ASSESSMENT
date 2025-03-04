using CRM.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAntiforgery();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthorization();

// Add debug logging for all requests
app.Use(async (context, next) =>
{
    Console.WriteLine($"DEBUG: Request received - Method: {context.Request.Method}, Path: {context.Request.Path}, Query: {context.Request.QueryString}");
    if (context.Request.Path == "/")
    {
        Console.WriteLine("DEBUG: Redirecting from / to /login.html");
        context.Response.Redirect("/login.html");
        return;
    }
    try
    {
        await next.Invoke();
        Console.WriteLine($"DEBUG: Request completed - Status: {context.Response.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DEBUG: Middleware error - {ex.Message}");
        Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");
        throw; 
    }
});

app.MapRazorPages();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MVCCases}/{action=Read}/{id?}");

app.Run();