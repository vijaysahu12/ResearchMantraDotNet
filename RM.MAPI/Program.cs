using RM.Database.ResearchMantraContext;
using RM.MService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var builder = WebApplication.CreateBuilder(args);
string connectionString = configuration.GetConnectionString("GurujiDevCS");

// Add services to the container.
builder.Services.AddDbContext<ResearchMantraContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddTransient<IAccountService, AccountService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
