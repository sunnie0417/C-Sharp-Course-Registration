var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Add Session
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

// Use Session 
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
