using Assignment.Core;
using AutoFixture.Xunit2;
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
        _repository = new WorkItemRepository(_context);
    }

    [Theory]
    [AutoData]
    public void Create_CreatesWorkItem_WhenGivenDetails(WorkItemCreateDTO dto)
    {
        dto = dto with { AssignedToId = null };

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
