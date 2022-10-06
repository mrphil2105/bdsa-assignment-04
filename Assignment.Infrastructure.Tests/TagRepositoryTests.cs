using Assignment.Core;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
        secondDto = secondDto with {Name = dto.Name};
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
      
      
    }






    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
