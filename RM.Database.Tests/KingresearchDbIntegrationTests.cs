using System.Linq;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RM.Database.Tests;

public class KingresearchDbIntegrationTests
{
    private RM.Database.KingResearchContext.KingResearchContext _context = null!;
    private DatabaseFixture _fixture = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _fixture = new DatabaseFixture();
        _context = _fixture.Context;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _fixture.Dispose();
    }


    [Test]
    public async Task Entities_Match_Table_And_Column_With_Type_Base_Check()
    {
        var IgnoredTypeMismatches = new HashSet<(string efType, string dbType)>
        {
            ("datetime2", "datetime"),
            ("nvarchar", "varchar"),
            ("varchar", "nvarchar"),
            //("bit", "int"),
            //("int", "bit"),
            ("nvarchar", "text"),
            ("nvarchar", "nchar"),
            ("nvarchar", "char"),
            ("real", "float"),
            //("int", "bigint"),
            ("date", "datetime2"),
            //("bigint", "int")
        };

        await using var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE 
        FROM INFORMATION_SCHEMA.COLUMNS";

        using var reader = await command.ExecuteReaderAsync();
        var schema = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync())
        {
            var table = reader.GetString(0);
            var column = reader.GetString(1);
            var dataType = reader.GetString(2);

            if (!schema.TryGetValue(table, out var columns))
            {
                columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                schema[table] = columns;
            }

            columns[column] = dataType; // only base type
        }

        var issues = new List<string>();

        foreach (var entity in _context.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (string.IsNullOrEmpty(tableName)) continue;

            if (!schema.ContainsKey(tableName))
            {
                issues.Add($"❌ Table '{tableName}' not found in the database.\n   ➕ Suggestion: Create table '{tableName}' in SQL DB to match EF model.");
                continue;
            }

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName(StoreObjectIdentifier.Table(tableName, null));
                if (string.IsNullOrEmpty(columnName)) continue;

                if (!schema[tableName].ContainsKey(columnName))
                {
                    issues.Add($"❌ Column '{columnName}' not found in table '{tableName}'.\n   ➕ Suggestion: Add column '{columnName}' to table '{tableName}' in SQL DB.");
                    continue;
                }

                var expectedType = property.GetColumnType() ?? property.FindRelationalTypeMapping()?.StoreType;
                if (!string.IsNullOrEmpty(expectedType))
                {
                    var efBaseType = expectedType.Split('(')[0].Trim().ToLowerInvariant();
                    var dbBaseType = schema[tableName][columnName].Trim().ToLowerInvariant();

                    if (efBaseType != dbBaseType)
                    {
                        bool ignore = IgnoredTypeMismatches.Contains((efBaseType, dbBaseType));
                        if (!ignore)
                        {
                            issues.Add(
                                $"⚠️ Type mismatch for table '{tableName}', column '{columnName}': EF type = '{efBaseType}', DB type = '{dbBaseType}'\n" +
                                $"   🔄 Suggestion: Alter column '{columnName}' in table '{tableName}' to match EF type '{efBaseType}', or adjust EF model if DB type is correct.");
                        }
                    }
                }
            }
        }

        Assert.True(issues.Count == 0, string.Join(Environment.NewLine, issues));
    }



}

