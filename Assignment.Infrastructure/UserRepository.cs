using AutoMapper;

namespace Assignment.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly KanbanContext _context;
    private readonly IMapper _mapper;

    public UserRepository(KanbanContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        var emailExists = _context.Users.Any(u => u.Email == user.Email);

        if (emailExists)
        {
            return (Conflict, 0);
        }

        var entity = _mapper.Map<User>(user);

        _context.Users.Add(entity);
        _context.SaveChanges();

        return (Created, entity.Id);
    }

    public UserDTO? Find(int userId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        throw new NotImplementedException();
    }

    public Response Update(UserUpdateDTO user)
    {
        throw new NotImplementedException();
    }

    public Response Delete(int userId, bool force = false)
    {
        throw new NotImplementedException();
    }
}
