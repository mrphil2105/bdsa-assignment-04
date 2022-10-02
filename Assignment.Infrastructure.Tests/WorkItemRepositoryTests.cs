using Assignment.Core;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KanbanContext _context;
    private readonly WorkItemRepository _repository;

    public WorkItemRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
        optionsBuilder.UseSqlite(_connection);

        _context = new KanbanContext(optionsBuilder.Options);
        _context.Database.EnsureCreated();

        var mapperConfig = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        mapperConfig.AssertConfigurationIsValid();
        var mapper = mapperConfig.CreateMapper();

        _repository = new WorkItemRepository(_context, mapper);
    }

    [Theory]
    [AutoDbData]
    public void Create_CreatesWorkItem_WhenGivenDetails(WorkItemCreateDTO dto)
    {
        var (response, id) = _repository.Create(dto);

        response.Should()
            .Be(Response.Created);
        id.Should()
            .Be(1);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
