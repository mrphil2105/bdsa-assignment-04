using Assignment.Core;
using AutoMapper;
using Microsoft.Data.Sqlite;

namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KanbanContext _context;
    private readonly IMapper _mapper;
    private readonly WorkItemRepository _repository;

    public WorkItemRepositoryTests()
    {
        (_connection, _context, _mapper, _repository) = TestsHelper.CreateTestObjects<WorkItemRepository>();
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
    public void Create_ReturnsBadRequest_WhenGivenInvalidAssignedToId(WorkItemCreateDTO dto, int assignedToId)
    {
        dto = dto with { AssignedToId = assignedToId };

        var (response, _) = _repository.Create(dto);

        response.Should()
            .Be(BadRequest);
    }

    [Theory]
    [AutoDbData]
    public void Create_ReturnsBadRequest_WhenGivenDuplicateTags(WorkItemCreateDTO dto)
    {
        dto.Tags.Add(dto.Tags.Last());

        var (response, _) = _repository.Create(dto);

        response.Should()
            .Be(BadRequest);
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

    [Theory]
    [AutoDbData]
    public void Update_UpdatesWorkItem_WhenGivenDetails(WorkItemCreateDTO createDto, WorkItemUpdateDTO updateDto)
    {
        var (_, id) = _repository.Create(createDto);
        updateDto = updateDto with { Id = id };

        var response = _repository.Update(updateDto);

        response.Should()
            .Be(Updated);
        _repository.Find(id)
            .Should()
            .BeEquivalentTo(updateDto, o => o.Excluding(d => d.AssignedToId));
    }

    [Theory]
    [AutoDbData]
    public void Update_ReturnsBadRequest_WhenGivenInvalidAssignedToId(WorkItemCreateDTO createDto,
        WorkItemUpdateDTO updateDto, int assignedToId)
    {
        var (_, id) = _repository.Create(createDto);
        updateDto = updateDto with { Id = id, AssignedToId = assignedToId };

        var response = _repository.Update(updateDto);

        response.Should()
            .Be(BadRequest);
    }

    [Theory]
    [AutoDbData]
    public void Update_ReturnsBadRequest_WhenGivenDuplicateTags(WorkItemCreateDTO createDto,
        WorkItemUpdateDTO updateDto)
    {
        var (_, id) = _repository.Create(createDto);
        updateDto = updateDto with { Id = id };
        updateDto.Tags.Add(updateDto.Tags.Last());

        var response = _repository.Update(updateDto);

        response.Should()
            .Be(BadRequest);
    }

    [Theory]
    [AutoDbData]
    public void Delete_DeletesWorkItem_WhenStateNew(WorkItemCreateDTO dto)
    {
        var (_, id) = _repository.Create(dto);

        var response = _repository.Delete(id);

        response.Should()
            .Be(Deleted);
        _context.Items.Should()
            .BeEmpty();
    }

    [Theory]
    [AutoDbData]
    public void Delete_MarksWorkItemRemoved_WhenStateActive(WorkItemCreateDTO dto)
    {
        var entity = _mapper.Map<WorkItem>(dto);
        entity.State = Active;
        _context.Items.Add(entity);
        _context.SaveChanges();

        var response = _repository.Delete(entity.Id);

        response.Should()
            .Be(Updated);
        _context.Items.Should()
            .ContainSingle(i => i.State == Removed);
    }

    [Theory]
    [AutoDbData]
    public void Delete_ReturnsConflict_WhenStateResolved(WorkItemCreateDTO dto)
    {
        var entity = _mapper.Map<WorkItem>(dto);
        entity.State = Resolved;
        _context.Items.Add(entity);
        _context.SaveChanges();

        var response = _repository.Delete(entity.Id);

        response.Should()
            .Be(Conflict);
        _context.Items.Should()
            .HaveCount(1);
    }

    [Theory]
    [AutoDbData]
    public void Delete_ReturnsConflict_WhenStateClosed(WorkItemCreateDTO dto)
    {
        var entity = _mapper.Map<WorkItem>(dto);
        entity.State = Closed;
        _context.Items.Add(entity);
        _context.SaveChanges();

        var response = _repository.Delete(entity.Id);

        response.Should()
            .Be(Conflict);
        _context.Items.Should()
            .HaveCount(1);
    }

    [Theory]
    [AutoDbData]
    public void Delete_ReturnsConflict_WhenStateRemoved(WorkItemCreateDTO dto)
    {
        var entity = _mapper.Map<WorkItem>(dto);
        entity.State = Removed;
        _context.Items.Add(entity);
        _context.SaveChanges();

        var response = _repository.Delete(entity.Id);

        response.Should()
            .Be(Conflict);
        _context.Items.Should()
            .HaveCount(1);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
