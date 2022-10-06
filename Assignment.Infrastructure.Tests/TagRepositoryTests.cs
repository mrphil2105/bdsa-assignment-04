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





    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
