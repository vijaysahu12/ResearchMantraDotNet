using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using RM.BlobStorage;
using RM.ChatGPT;
using RM.CommonService;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.MobileAPI.Helpers;
using RM.Model.MongoDbCollection;
using RM.MService.Services;
using RM.NotificationService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Quartz;
using Serilog;
using System.Security.Claims;
using System.Text;



var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();


// Add services to the container.

#region Injecting Services
string connectionString = configuration.GetConnectionString("GurujiDevCS")!;

//builder.Services.AddDbContext<KingResearchContext>(options =>
//    options.UseSqlServer(connectionString)
//           .EnableSensitiveDataLogging() // Log detailed query data
//           .LogTo(Console.WriteLine));  // Log queries and context lifecycle to the console

// Read allowed origin from appsettings.json
builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
    var mongoSettings = provider.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var client = new MongoClient(mongoSettings.ConnectionURI);
    return client.GetDatabase(mongoSettings.DatabaseName);
});
builder.Services.AddHttpClient();
builder.Services.AddDbContext<KingResearchContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<CommunityPostService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMobileNotificationService, MobileNotificationService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<IResearchService, ResearchService>();
builder.Services.AddScoped<IOtherService, OtherService>();
builder.Services.AddScoped<IPurchaseOrderMService, PurchaseorderMService>();
builder.Services.AddScoped<IPusherChannelService, PusherChannelService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<MongoDbService>();
builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
builder.Services.AddSingleton<StockMarketContractsService>();
builder.Services.AddScoped<ScreenerService>();
builder.Services.AddScoped<SubscriptionPlanService>();
builder.Services.AddSingleton<FirebaseRealTimeDb>();
builder.Services.AddSingleton<FirebaseNotification>();
builder.Services.AddScoped<PreAndPostMarketService>();
builder.Services.AddScoped<SchedulerServiceMobile>();
builder.Services.AddScoped<IEmailService, BrevoEmailService>();
builder.Services.AddScoped<StockInsightService>();
builder.Services.AddScoped(typeof(Lazy<IMobileNotificationService>), serviceProvider =>
{
    return new Lazy<IMobileNotificationService>(() => serviceProvider.GetRequiredService<IMobileNotificationService>());
});

#endregion

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.Configure<BlobStorageConfigModel>(builder.Configuration.GetSection("Azure"));
builder.Services.Configure<BrevoSettings>(builder.Configuration.GetSection("Brevo"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();



#region CRON JOB REGISTRATION

//builder.Services.AddQuartz(q =>
//{
//    q.UseMicrosoftDependencyInjectionJobFactory();
//    q.AddJob<ResetFirebaseDataCronJob>(opt => opt.WithIdentity("SubscriptionTopicExpiryNotificationCheckJob"));
//    q.AddTrigger(opt => opt.ForJob("SubscriptionTopicExpiryNotificationCheckJob")
//    .WithIdentity("SubscriptionTopicExpiryNotificationCheckJobTrigger")
//    .WithCronSchedule(configuration["CronJobSchedule:SubscriptionTopicExpiryNotificationCheckJob"]!));
//});

//builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Swagger configuration
#region AddSwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mobile API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer {token}')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

//JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha512 }, // Force HS512
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value!)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true, // Ensure token expiration is validated
        ClockSkew = TimeSpan.Zero,
        RequireSignedTokens = false,

    };
    options.Events = new JwtBearerEvents
    {
        // Generate random numbers

        OnTokenValidated = async context =>
        {
            var _mongoDb = context.HttpContext.RequestServices.GetRequiredService<MongoDbService>();
            var mobileUserId = context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
            var mobileUserKey = context.Principal?.Claims.FirstOrDefault(c => c.Type == "userPublicKey")?.Value;
            var version = context.Principal?.Claims.FirstOrDefault(c => c.Type == "version")?.Value;
            var deviceType = context.Principal?.Claims.FirstOrDefault(c => c.Type == "deviceType")?.Value;

            await _mongoDb.ManageUserVersionReport("OnTokenValidation", deviceType, version, mobileUserKey, Convert.ToInt64(mobileUserId));
        },
        //OnAuthenticationFailed = async context =>
        //        {
        //            var _mongoDb = context.HttpContext.RequestServices.GetRequiredService<IMongoRepository<ExceptionLog>>();
        //            var accessToken = context.Request.Headers["Authorization"].ToString();

        //            var mobileUserId = context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
        //            var mobileUserKey = context.Principal?.Claims.FirstOrDefault(c => c.Type == "userPublicKey")?.Value;

        //            try
        //            {
        //                var payloadJson = DecodeJwtPayload(accessToken);

        //                if (payloadJson == null)
        //                {
        //                    payloadJson = accessToken;
        //                }

        //                await _mongoDb.AddAsync(new ExceptionLog
        //                {
        //                    CreatedOn = DateTime.Now,
        //                    InnerException = context.Exception?.InnerException?.ToString(),
        //                    Message = context.Exception?.Message,
        //                    StackTrace = context.Exception?.StackTrace,
        //                    RequestBody = context.Response.StatusCode + " : " + mobileUserId + " : " + mobileUserKey + " : " + payloadJson,
        //                    Source = "JWT"
        //                });
        //            }
        //            catch
        //            {
        //                Console.Write("Catch while parsing jwt token");
        //            }


        //            //// Ensure response modification is only done if the response has not started
        //            //if (!context.Response.HasStarted)
        //            //{
        //            //    // Return 401 Unauthorized
        //            //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //            //    context.Response.ContentType = "application/json";

        //            //    // Customize the response body if needed
        //            //    var result = System.Text.Json.JsonSerializer.Serialize(new
        //            //    {
        //            //        error = "Authentication failed",
        //            //        details = context.Exception.Message
        //            //    });

        //            //    await context.Response.WriteAsync(result);
        //            //}
        //        }
    };
});


//static string DecodeJwtPayload(string token)
//{
//    // Split the token into its parts
//    string[] tokenParts = token.Split('.');
//    if (tokenParts.Length < 2)
//    {
//        return null;
//        //throw new ArgumentException("Invalid JWT token");
//    }

//    // The payload is the second part (Base64Url encoded)
//    string payload = tokenParts[1];

//    // Decode Base64Url to Base64
//    string base64 = payload.Replace('-', '+').Replace('_', '/');

//    // Add padding if necessary
//    switch (base64.Length % 4)
//    {
//        case 2: base64 += "=="; break;
//        case 3: base64 += "="; break;
//    }

//    // Convert Base64 to a byte array
//    byte[] bytes = Convert.FromBase64String(base64);

//    // Convert byte array to string (JSON)
//    return Encoding.UTF8.GetString(bytes);
//}
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddlewareNew>();

//var firebaseSetting = "prod_firebase.json";
var firebaseSetting = "local_firebase.json";
//if (app.Environment.EnvironmentName == "Development")
//{
//    firebaseSetting = "local_firebase.json";
//}
//var path = @"E:\KING_CRM\BackUpCrm\kingcrm\DotNet\RM.MobileAPI\local_firebase.json";
//var path = @"C:\Users\CodeLine1\Documents\Vijay Sahu\Coding\kingcrm\DotNet\RM.MobileAPI\local_firebase.json";


//#if DEBUG
//firebaseSetting = "local_firebase.json";
//#endif

FirebaseApp.Create(new AppOptions()
{
   Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, firebaseSetting)),
   //Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)),
});


var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Environment: {EnvironmentName} , FireBaseSetting: {firebaseSetting}", app.Environment.EnvironmentName, firebaseSetting);



app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials());

app.UseStaticFiles(); // Enable static files middleware

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.UseDeveloperExceptionPage();


app.Use(async (context, next) =>
{
    try
    {
        // Get MongoDB service safely
        var _mongoRepo = context.RequestServices.GetService<IMongoRepository<RM.Model.MongoDbCollection.Log>>();

        if (_mongoRepo == null)
        {
            await next();
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault() ?? "No Token Provided";
        var mobileUserId = context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value ?? "No UserId";
        var mobileUserKey = context.User?.Claims.FirstOrDefault(c => c.Type == "userPublicKey")?.Value ?? "No UserKey";


        //await _mongoRepo.AddAsync(new RM.Model.MongoDbCollection.Log
        //{
        //    CreatedOn = DateTime.Now,
        //    Message = $"MobileUserId: {mobileUserId} MobileUserKey: {mobileUserKey} Token: {token}",
        //    Source = "JWT",
        //    Category = "JWT"
        //});
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in JWT Middleware: {ex.Message}");
    }

    await next();
});
app.Run();

