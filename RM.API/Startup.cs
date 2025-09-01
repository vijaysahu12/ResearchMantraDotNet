using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using RM.API.Helpers;
using RM.API.Hub;
using RM.API.Interfaces;
using RM.API.Models.Mail;
using RM.API.Services;
using RM.BlobStorage;
using RM.ChatGPT;
using RM.CommonService;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.MService.Services;
using RM.NotificationService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Quartz;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        [Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            // Read allowed origin from appsettings.json
            services.AddSingleton<IMongoDatabase>(provider =>
            {
                var mongoSettings = provider.GetRequiredService<IOptions<MongoDBSettings>>().Value;
                var client = new MongoClient(mongoSettings.ConnectionURI);
                return client.GetDatabase(mongoSettings.DatabaseName);
            });

            _ = services.AddDbContext<KingResearchContext>(options => options.UseSqlServer(Configuration.GetConnectionString("GurujiDevCS")));
            _ = services.AddControllers();
            var allowedOrigins = Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            _ = services.AddCors(options =>
            {
                options.AddPolicy("MyKingResearchCorsPolicy", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
            _ = services.AddMemoryCache();
            _ = services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            _ = services.Configure<MailSettingsMarketManthan>(Configuration.GetSection("MailSettingsMarketManthan"));
            _ = services.AddHttpClient();  
            _ = services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

            _ = services.AddScoped<IAuthRepository, AuthRepository>();
            _ = services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            _ = services.AddScoped<ICallPerformanceService, CallPerformanceService>();
            _ = services.AddScoped<IStocksService, StocksService>();
            _ = services.AddScoped<IWatiApiService, WatiApiService>();
            _ = services.AddScoped<IPartnerService, PartnerService>();
            
            _ = services.AddScoped<ISchedulerService, SchedulerService>();
            _ = services.AddScoped<IMobileService, MobileService>();
            _ = services.AddTransient<IActivityService, ActivityService>();
            _ = services.AddScoped<ICustomerService, Services.CustomerService>();
            _ = services.AddTransient<IMailService, MailService>();
            _ = services.AddScoped<IMobileNotificationService, MobileNotificationService>();

            services.AddScoped(typeof(Lazy<IMobileNotificationService>), serviceProvider =>
            {
                return new Lazy<IMobileNotificationService>(() => serviceProvider.GetRequiredService<IMobileNotificationService>());
            });

            _ = services.AddScoped<IPushNotificationService, PushNotificationService>();
            _ = services.AddSingleton<FirebaseRealTimeDb>();
            _ = services.AddScoped<SchedulerServiceMobile>();
            _ = services.AddScoped<FirebaseNotification>();
            _ = services.AddTransient<CommunityPostService>();
            _ = services.AddScoped<StockInsightService>();
            _ = services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
            _ = services.AddScoped<ILeadService, LeadService>();
            _ = services.AddScoped<IKycService, KycService>();
            //_ = services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDB"));
            _ = services.Configure<BlobStorageConfigModel>(Configuration.GetSection("Azure"));
            //_ = services.AddScoped<MongoDbService>();
            _ = services.AddScoped<ILearningMaterialService, LearningMaterialService>();
            _ = services.AddScoped<Services.IDashboardService, Services.DashboardService>();

            _ = services.AddScoped<IBlogService, BlogService>();
            _ = services.AddSingleton<StockMarketContractsService>();
            _ = services.AddScoped<MongoDbService>();
            _ = services.AddScoped<LandingPageService>();
            _ = services.AddHttpContextAccessor();

            services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDB"));
            //DashboardService



            //services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            //services.AddSingleton<IJobFactory, JobFactory>();
            //services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            //services.AddSingleton<PartnerSessionJob>();
            //services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(PartnerSessionJob), "GetPartnerSession", "0/10 * * * * ?"));
            //services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(PartnerSessionJob), "GetPartnerSession", "0 0 11 * * ?"));
            //services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(LeadExpiryCheckJob), "LeadExpiryCheckJob", "0 40 13 * * ?"));
            //services.AddHostedService<PartnerScheduler>();

            //// ASP.NET Core hosting
            //_ = services.AddQuartzServer(options =>
            //{
            //    options.WaitForJobsToComplete = true;
            //});

            //_ = services.AddQuartz(q =>
            //{
            //    q.UseMicrosoftDependencyInjectionScopedJobFactory();

            //    // Just use the name of your job that you created in the Jobs folder.
            //    JobKey jobKeyUnTochedLeadAssignedToNullReminderJob = new("GetUnTochedLeadAssignedToNullJob");
            //    _ = q.AddJob<UnTochedLeadAssignedToNullReminderJob>(opts => opts.WithIdentity(jobKeyUnTochedLeadAssignedToNullReminderJob));
            //    _ = q.AddTrigger(opts => opts
            //      .ForJob(jobKeyUnTochedLeadAssignedToNullReminderJob)
            //      .WithIdentity("SendEmailJob-trigger")
            //      //This Cron interval can be described as "run every minute" (when second is zero)
            //      .WithCronSchedule(Configuration.GetSection("JobSchedulerCron:UnTochedLeadAssignedToNullReminderJob").Value));

            //    JobKey jobKeyLeadsFollowUpReminder = new("LeadsFollowUpReminder");
            //    _ = q.AddJob<LeadsFollowUpReminder>(opts => opts.WithIdentity(jobKeyLeadsFollowUpReminder));
            //    _ = q.AddTrigger(opts => opts
            //      .ForJob(jobKeyLeadsFollowUpReminder)
            //      .WithIdentity("LeadsFollowUpReminder-trigger")
            //      .WithCronSchedule(Configuration.GetSection("JobSchedulerCron:LeadsFollowUpReminder").Value));

            //    JobKey UpdateUntouchedLeadsToNullJobsKey = new("UpdateUntouchedLeadsToNullJobsKey");
            //    _ = q.AddJob<UpdateUntouchedLeadsToNullJobs>(opts => opts.WithIdentity(UpdateUntouchedLeadsToNullJobsKey));
            //    _ = q.AddTrigger(opts => opts
            //      .ForJob(UpdateUntouchedLeadsToNullJobsKey)
            //      .WithIdentity("UpdateUntouchedLeadsToNull-trigger")
            //      .WithCronSchedule(Configuration.GetSection("JobSchedulerCron:UpdateUntouchedLeadsToNullJobs").Value));

            //    JobKey customerServiceExpiryReminderJob = new("CustomerServiceExpiryReminderJob");
            //    _ = q.AddJob<CustomerServiceExpiryReminderJob>(opts => opts.WithIdentity(customerServiceExpiryReminderJob));
            //    _ = q.AddTrigger(opts => opts
            //      .ForJob(customerServiceExpiryReminderJob)
            //      .WithIdentity("GetPartnerSession-trigger")
            //      //This Cron interval can be described as "run every minute" (when second is zero)
            //      .WithCronSchedule(Configuration.GetSection("JobSchedulerCron:CustomerServiceExpiryReminderJob").Value));

            //    JobKey UpdateExpiredServiceJobs = new("UpdateExpiredServiceJobs");
            //    _ = q.AddJob<CustomerServiceExpiryUpdateJob>(opts => opts.WithIdentity(UpdateExpiredServiceJobs));
            //    _ = q.AddTrigger(opts => opts
            //      .ForJob(UpdateExpiredServiceJobs)
            //      .WithIdentity("UpdateExpiredServiceJobs")
            //      .WithCronSchedule(Configuration.GetSection("JobSchedulerCron:UpdateExpiredServiceJobs").Value));

            //    JobKey UpdateContractsJob = new("UpdateContractsJob");
            //    _ = q.AddJob<UpdateContractsJob>(opts => opts.WithIdentity(UpdateContractsJob));
            //    _ = q.AddTrigger(opts => opts
            //      .ForJob(UpdateContractsJob)
            //      .WithIdentity("UpdateContractsJob-trigger")
            //      //This Cron interval can be described as "run every minute" (when second is zero)
            //      .WithCronSchedule(Configuration.GetSection("JobSchedulerCron:UpdateContractsJobs").Value));
            //});

            //_ = services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            _ = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                        .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    //IF you comment the below code then it will send the push notification to UI beacuse of not found the access token.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            Microsoft.Extensions.Primitives.StringValues accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"JWT Authentication Failed: {context.Exception}");
                            Console.WriteLine("Check token getting failed here");
                            return Task.CompletedTask;
                        }
                    };
                });

            _ = services.AddSignalR();


            #region AddSwaggerGen
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "KingResearch CRM API", Version = "v1" });

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

            _ = services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });

            // Auto Mapper Configurations
            MapperConfiguration mappingConfig = new(mc =>
            {
                mc.AddProfile(new MappingProfileHelper());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            _ = services.AddSingleton(mapper);


            //var firebaseSetting = "prod_firebase.json";
            var firebaseSetting = "local_firebase.json";
            //if (app.Environment.EnvironmentName == "Development")
            //{
            //    firebaseSetting = "local_firebase.json";
            //}

//#if DEBUG
//            firebaseSetting = "local_firebase.json";
//#endif

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, firebaseSetting)),
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<GlobalExceptionMiddlewareNew>();

            _ = app.UseRouting();

            app.UseCors("MyKingResearchCorsPolicy");

            app.Use(async (context, next) =>
            {
                var requestComingFrom = context.Request.Headers["User-Agent"].ToString();
                string[] allowedKeywords = { "Mozilla", "Chrome", "Safari", "AppleWebKit" };

                // Check if the User-Agent contains any of the allowed keywords
                //Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 132.0.0.0 Safari / 537.36

                if (!string.IsNullOrEmpty(requestComingFrom) && !allowedKeywords.Any(keyword => requestComingFrom.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid API Key.");
                    return;
                }

                await next();
            });
            //_ = app.UseCors(x => x
            // .AllowAnyMethod()
            // .AllowAnyHeader()
            // .SetIsOriginAllowed(origin => true) // allow any origin
            // .AllowCredentials()); // allow credentials

            _ = app.UseAuthentication();
            _ = app.UseAuthorization();
            app.UseHttpsRedirection();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
                _ = endpoints.MapHub<NotificationHub>("/notification");
            });

            _ = app.UseSwagger();
            _ = app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "KingResearch API V1");
            });
        }
    }
}