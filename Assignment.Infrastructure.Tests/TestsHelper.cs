using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Infrastructure.Tests;

public static class TestsHelper
{
    public static (SqliteConnection, KanbanContext, IMapper, TRepository) CreateTestObjects<TRepository>()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
        optionsBuilder.UseSqlite(connection);

        var context = new KanbanContext(optionsBuilder.Options);
        context.Database.EnsureCreated();

        var mapperConfig = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        mapperConfig.AssertConfigurationIsValid();
        var mapper = mapperConfig.CreateMapper();

        var repository = (TRepository)Activator.CreateInstance(typeof(TRepository), context, mapper)!;

        return (connection, context, mapper, repository);
    }
}
