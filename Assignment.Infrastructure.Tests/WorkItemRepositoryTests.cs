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
            .Be(Created);
        id.Should()
            .Be(1);
    }

    [Theory]
    [AutoDbData]
    public void Create_CreatesWorkItem_WhenGivenDuplicateDetails(WorkItemCreateDTO dto)
    {
        _repository.Create(dto);

        var (response, id) = _repository.Create(dto);

        response.Should()
            .Be(Created);
        id.Should()
            .Be(2);
    }

    [Theory]
    [AutoDbData]
    public void Create_SetsCreatedAndStateUpdated_WhenGivenDetails(WorkItemCreateDTO dto)
    {
        var expected = DateTime.UtcNow;

        var (_, id) = _repository.Create(dto);

        var entity = _context.Items.Find(id);
        entity!.Created.Should()
            .BeCloseTo(expected, TimeSpan.FromSeconds(1));
        entity.StateUpdated.Should()
            .BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoDbData]
    public void Find_ReturnsWorkItemDetailsDTO_WhenGivenId(WorkItemCreateDTO dto)
    {
        var (_, id) = _repository.Create(dto);

        var result = _repository.Find(id);

        result.Should()
            .BeEquivalentTo(dto, o => o.Excluding(d => d.AssignedToId));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
