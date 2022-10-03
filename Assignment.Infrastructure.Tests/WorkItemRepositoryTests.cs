using Assignment.Core;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KanbanContext _context;
    private readonly IMapper _mapper;
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
        _mapper = mapperConfig.CreateMapper();

        _repository = new WorkItemRepository(_context, _mapper);
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

    [Theory]
    [AutoDbData]
    public void Read_ReturnsWorkItemDTOs_WhenCreated(List<WorkItemCreateDTO> dtos)
    {
        dtos.ForEach(d => _repository.Create(d));

        var result = _repository.Read();

        result.Should()
            .BeEquivalentTo(dtos, o => o.Excluding(d => d.AssignedToId)
                .Excluding(d => d.Description));
    }

    [Theory]
    [AutoDbData]
    public void ReadRemoved_ReturnsRemovedWorkItemDTOs_WhenRemoved(List<WorkItemCreateDTO> dtos)
    {
        var entities = dtos.Select((d, i) =>
        {
            var entity = _mapper.Map<WorkItem>(d);

            if (i % 2 == 0)
            {
                entity.State = Removed;
            }

            return entity;
        });
        _context.Items.AddRange(entities);
        _context.SaveChanges();

        var result = _repository.ReadRemoved();

        result.Should()
            .BeEquivalentTo(dtos.Where((_, i) => i % 2 == 0), o => o.Excluding(d => d.AssignedToId)
                .Excluding(d => d.Description));
    }

    [Theory]
    [AutoDbData]
    public void ReadByTag_ReturnsMatchingWorkItemDTOs_WhenCreated(List<WorkItemCreateDTO> dtos, string tag)
    {
        dtos.ForEach(d =>
        {
            var index = dtos.IndexOf(d);

            if (index % 2 == 0)
            {
                d.Tags.Add(tag);
            }

            _repository.Create(d);
        });

        var result = _repository.ReadByTag(tag);

        result.Should()
            .BeEquivalentTo(dtos.Where((_, i) => i % 2 == 0), o => o.Excluding(d => d.AssignedToId)
                .Excluding(d => d.Description));
    }

    [Theory]
    [AutoDbData]
    public void ReadByUser_ReturnsMatchingWorkItemDTOs_WhenCreated(List<WorkItemCreateDTO> dtos, string username,
        string email)
    {
        _context.Users.Add(new User(username, email));
        _context.SaveChanges();
        dtos = dtos.Select((d, i) =>
            {
                d = i % 2 == 0 ? d with { AssignedToId = 1 } : d;
                _repository.Create(d);

                return d;
            })
            .ToList();

        var result = _repository.ReadByUser(1);

        result.Should()
            .BeEquivalentTo(dtos.Where((_, i) => i % 2 == 0), o => o.Excluding(d => d.AssignedToId)
                .Excluding(d => d.Description));
    }

    [Theory]
    [AutoDbData]
    public void ReadByState_ReturnsMatchingWorkItemDTOs_WhenCreated(List<WorkItemCreateDTO> dtos)
    {
        var entities = dtos.Select((d, i) =>
        {
            var entity = _mapper.Map<WorkItem>(d);

            if (i % 2 == 0)
            {
                entity.State = Active;
            }

            return entity;
        });
        _context.Items.AddRange(entities);
        _context.SaveChanges();

        var result = _repository.ReadByState(Active);

        result.Should()
            .BeEquivalentTo(dtos.Where((_, i) => i % 2 == 0), o => o.Excluding(d => d.AssignedToId)
                .Excluding(d => d.Description));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
