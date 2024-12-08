// Tutorial followed for the start of this to get the identity framework in: https://www.telerik.com/blogs/new-net-8-aspnet-core-identity-how-implement

using IdentityManager.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Connection info stored in appsettings.json
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
	opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
	opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Please enter token",
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		BearerFormat = "JWT",
		Scheme = "bearer"
	});

	opt.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type=ReferenceType.SecurityScheme,
					Id="Bearer"
				}
			},
			new string[]{}
		}
	});
});
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(configuration["ConnectionStrings:DefaultSQLiteConnection"]));
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // Apply migrations for ApplicationDbContext
        var identityDbContext = services.GetRequiredService<ApplicationDbContext>();
        identityDbContext.Database.Migrate();

        // Apply migrations for DataContext
        var customDbContext = services.GetRequiredService<DataContext>();
        customDbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the error (use a logging framework in production apps)
        Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGroup("/identity").MapIdentityApi<IdentityUser>();

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
	var forecast = Enumerable.Range(1, 5).Select(index =>
		new WeatherForecast
		(
			DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
			Random.Shared.Next(-20, 55),
			summaries[Random.Shared.Next(summaries.Length)]
		))
		.ToArray();
	return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapGet("/localLocations/all", (DataContext context) => {
	return context.LocalLocations.ToList();
})
// .RequireAuthorization()
.WithName("GetAllLocations");

app.MapPost("/localLocations/", async (DataContext context, [Microsoft.AspNetCore.Mvc.FromBody] LocalLocation localLocation) => {
    context.Add(localLocation);
    await context.SaveChangesAsync();
    // await _hubContext.Clients.All.SendAsync("ReceiveAddMessage", country);
	// return context.LocalLocations.ToList();
})
// .RequireAuthorization()
.WithName("PostLocation");

app.MapDelete("/localLocations/{id}", async (int localLocationId, DataContext context) => {
	var localLocation = context.LocalLocations.Where(l => l.Id == localLocationId).FirstOrDefault();

    if (localLocation == null)
    {
        return Results.NotFound(new { Message = $"The localLocation of id {localLocationId} was not found." });
    }

	context.Remove(localLocation);
	await context.SaveChangesAsync();

    return Results.Ok(localLocation);
})
// .RequireAuthorization()
.WithName("DeleteLocation");


app.MapGet("/user/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(userId);

    if (user == null)
    {
        return Results.NotFound(new { Message = $"The user of id {userId} was not found." });
    }

    return Results.Ok(new
    {
        user.Id, //Keep? They would already have this
        user.UserName,
        user.Email,
    });
})
.RequireAuthorization()
.WithName("GetUser")
.WithOpenApi();




app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
  