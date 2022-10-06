using Assignment.Core;
using AutoMapper;
using Microsoft.Data.Sqlite;

namespace Assignment.Infrastructure.Tests;

public class UserRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly KanbanContext _context;
    private readonly IMapper _mapper;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        (_connection, _context, _mapper, _repository) = TestsHelper.CreateTestObjects<UserRepository>();
    }

    [Theory]
    [AutoDbData]
    public void Create_CreatesUser_WhenGivenDetails(UserCreateDTO dto)
    {
        var (response, id) = _repository.Create(dto);

        response.Should()
            .Be(Created);
        id.Should()
            .Be(1);
    }

    [Theory]
    [AutoDbData]
    public void Create_ReturnsConflict_WhenGivenExistingEmail(UserCreateDTO firstDto, UserCreateDTO secondDto)
    {
        secondDto = secondDto with { Email = firstDto.Email };
        _repository.Create(firstDto);

        var (response, _) = _repository.Create(secondDto);

        response.Should()
            .Be(Conflict);
    }

    [Theory]
    [AutoDbData]
    public void Find_ReturnsUserDTO_WhenGivenId(UserCreateDTO dto)
    {
        var (_, id) = _repository.Create(dto);

        var result = _repository.Find(id);

        result.Should()
            .BeEquivalentTo(dto);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
