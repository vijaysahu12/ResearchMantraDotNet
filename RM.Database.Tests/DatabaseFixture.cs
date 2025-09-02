using RM.Database.ResearchMantraContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace RM.Database.Tests;
    
public class DatabaseFixture : IDisposable
{
    public RM.Database.ResearchMantraContext.ResearchMantraContext Context { get; }

    public DatabaseFixture()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("GurujiDevCS")!;
        var options = new DbContextOptionsBuilder<RM.Database.ResearchMantraContext.ResearchMantraContext>()
            .UseSqlServer(connectionString)
            .Options;
        Context = new RM.Database.ResearchMantraContext.ResearchMantraContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
