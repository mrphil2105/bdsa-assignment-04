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

    [Theory]
    [AutoDbData]
    public void Read_ReturnsUserDTOs_WhenCreated(List<UserCreateDTO> dtos)
    {
        dtos.ForEach(d => _repository.Create(d));

        var result = _repository.Read();

        result.Should()
            .BeEquivalentTo(dtos);
    }

    [Theory]
    [AutoDbData]
    public void Update_UpdatesUser_WhenGivenDetails(UserCreateDTO createDto, UserUpdateDTO updateDto)
    {
        var (_, id) = _repository.Create(createDto);
        updateDto = updateDto with { Id = id };

        var response = _repository.Update(updateDto);

        response.Should()
            .Be(Updated);
        _repository.Find(id)
            .Should()
            .BeEquivalentTo(updateDto);
    }

    [Theory]
    [AutoDbData]
    public void Update_ReturnsConflict_WhenGivenExistingEmail(UserCreateDTO firstDto, UserCreateDTO secondDto,
        UserUpdateDTO updateDto)
    {
        _repository.Create(firstDto);
        var (_, id) = _repository.Create(secondDto);
        updateDto = updateDto with { Id = id, Email = firstDto.Email };

        var response = _repository.Update(updateDto);

        response.Should()
            .Be(Conflict);
    }

    [Theory]
    [AutoDbData]
    public void Delete_DeletesUser_WhenHasNoItems(UserCreateDTO dto)
    {
        var (_, id) = _repository.Create(dto);

        var response = _repository.Delete(id);

        response.Should()
            .Be(Deleted);
        _context.Users.Should()
            .BeEmpty();
    }

    [Theory]
    [AutoDbData]
    public void Delete_ReturnsConflict_WhenHasItems(UserCreateDTO userDto, WorkItemCreateDTO itemDto)
    {
        var user = _mapper.Map<User>(userDto);
        var item = _mapper.Map<WorkItem>(itemDto);
        user.Items.Add(item);
        _context.Users.Add(user);
        _context.SaveChanges();

        var response = _repository.Delete(user.Id);

        response.Should()
            .Be(Conflict);
        _context.Users.Should()
            .HaveCount(1);
    }

    [Theory]
    [AutoDbData]
    public void Delete_DeletesUser_WhenHasItemsAndForce(UserCreateDTO userDto, WorkItemCreateDTO itemDto)
    {
        var user = _mapper.Map<User>(userDto);
        var item = _mapper.Map<WorkItem>(itemDto);
        user.Items.Add(item);
        _context.Users.Add(user);
        _context.SaveChanges();

        var response = _repository.Delete(user.Id, true);

        response.Should()
            .Be(Deleted);
        _context.Users.Should()
            .BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
