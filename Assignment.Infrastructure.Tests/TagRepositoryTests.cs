using Assignment.Core;
using AutoMapper;
using Microsoft.Data.Sqlite;

namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KanbanContext _context;
    private readonly IMapper _mapper;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        (_connection, _context, _mapper, _repository) = TestsHelper.CreateTestObjects<TagRepository>();
    }

    [Theory]
    [AutoDbData]
    public void Create_CreatesTag_WhenGivenDetails(TagCreateDTO dto)
    {
        var (response, id) = _repository.Create(dto);

        response.Should()
            .Be(Created);
        id.Should()
            .Be(1);
    }

    [Theory]
    [AutoDbData]
    public void Create_ReturnsConflict_WhenGivenExistingName(TagCreateDTO dto, TagCreateDTO secondDto)
    {
        secondDto = secondDto with { Name = dto.Name };
        _repository.Create(dto);

        var (response, id) = _repository.Create(secondDto);

        response.Should()
            .Be(Conflict);
        id.Should()
            .Be(0);
    }

    [Theory]
    [AutoDbData]
    public void Read_ReturnsTagDTOs_WhenCalled(List<TagCreateDTO> dtos)
    {
        dtos.ForEach(d => _repository.Create(d));

        var result = _repository.Read();

        result.Should()
            .BeEquivalentTo(dtos);
    }

    [Theory]
    [AutoDbData]
    public void Find_ReturnsTagDTO_WhenGivenDetail(TagCreateDTO dto)
    {
        var (_, id) = _repository.Create(dto);

        var result = _repository.Find(id);

        result.Should()
            .BeEquivalentTo(dto);
    }

    [Theory]
    [AutoDbData]
    public void Update_UpdatesTag_WhenGivenDetails(TagCreateDTO dto, TagUpdateDTO updateDto)
    {
        var (_, id) = _repository.Create(dto);
        updateDto = updateDto with { Id = id };

        var response = _repository.Update(updateDto);

        _repository.Find(id)
            .Should()
            .BeEquivalentTo(updateDto);
        response.Should()
            .Be(Updated);
    }

    [Theory]
    [AutoDbData]
    public void Update_ReturnsConflict_WhenGivenExistingName(TagCreateDTO firstDto, TagCreateDTO secondDto,
        TagUpdateDTO updateDto)
    {
        _repository.Create(firstDto);
        var (_, id) = _repository.Create(secondDto);
        updateDto = updateDto with { Id = id, Name = firstDto.Name };

        var response = _repository.Update(updateDto);

        response.Should()
            .Be(Conflict);
    }

    [Theory]
    [AutoDbData]
    public void Update_ReturnNotFound_WhenTagDTONotFound(TagCreateDTO dto, TagUpdateDTO updateDto)
    {
        _repository.Create(dto);
        updateDto = updateDto with { Id = 2 };

        var response = _repository.Update(updateDto);

        response.Should()
            .Be(NotFound);
    }

    [Theory]
    [AutoDbData]
    public void Delete_DeletesTag_WhenHasNoItems(TagCreateDTO dto)
    {
        var (_, id) = _repository.Create(dto);

        var response = _repository.Delete(id);

        response.Should()
            .Be(Deleted);
        _context.Tags.Should()
            .BeEmpty();
    }

    [Theory]
    [AutoDbData]
    public void Delete_ReturnsNotFound_WhenTagDTONotFound(int id)
    {
        var response = _repository.Delete(id);

        response.Should()
            .Be(NotFound);
    }

    [Theory]
    [AutoDbData]
    public void Delete_ReturnsConflict_WhenHasItems(TagCreateDTO dto, WorkItemCreateDTO workDto)
    {
        var tag = _mapper.Map<Tag>(dto);
        var item = _mapper.Map<WorkItem>(workDto);
        tag.WorkItems.Add(item);
        _context.Tags.Add(tag);
        _context.SaveChanges();

        var response = _repository.Delete(tag.Id);

        response.Should()
            .Be(Conflict);
        _context.Tags.Should()
            .HaveCount(1);
    }

    [Theory]
    [AutoDbData]
    public void Delete_DeletesTag_WhenHasItemsAndForce(TagCreateDTO dto, WorkItemCreateDTO workDto)
    {
        var tag = _mapper.Map<Tag>(dto);
        var item = _mapper.Map<WorkItem>(workDto);
        tag.WorkItems.Add(item);
        _context.Tags.Add(tag);
        _context.SaveChanges();

        var response = _repository.Delete(tag.Id, true);

        response.Should()
            .Be(Deleted);
        _context.Tags.Should()
            .BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
