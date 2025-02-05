//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using Service;

var builder = WebApplication.CreateBuilder(args);

// הוספת הגדרות (Options) מהקובץ appsettings.json או מה-user secrets
builder.Services.Configure<GitHubService>(builder.Configuration.GetSection("GitHubSettings"));

// הוספת In-Memory Caching (לשימוש מאוחר יותר)
builder.Services.AddMemoryCache();

// רישום השירותים (נרשם בהמשך)
builder.Services.AddScoped<IGitHubService, GitHubService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
