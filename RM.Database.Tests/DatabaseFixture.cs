using RM.Database.KingResearchContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace RM.Database.Tests;
    
public class DatabaseFixture : IDisposable
{
    public RM.Database.KingResearchContext.KingResearchContext Context { get; }

    public DatabaseFixture()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("GurujiDevCS")!;
        var options = new DbContextOptionsBuilder<RM.Database.KingResearchContext.KingResearchContext>()
            .UseSqlServer(connectionString)
            .Options;
        Context = new RM.Database.KingResearchContext.KingResearchContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
