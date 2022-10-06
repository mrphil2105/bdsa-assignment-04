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
        return _mapper.Map<UserDTO>(_context.Users.Find(userId));
    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        return _context.Users.Select(i => _mapper.Map<UserDTO>(i))
            .ToList();
    }

    public Response Update(UserUpdateDTO user)
    {
        var entity = _context.Users.Find(user.Id);

        if (entity == null)
        {
            return NotFound;
        }

        var emailExists = _context.Users.Any(u => u.Id != user.Id && u.Email == user.Email);

        if (emailExists)
        {
            return Conflict;
        }

        _mapper.Map(user, entity);
        _context.SaveChanges();

        return Updated;
    }

    public Response Delete(int userId, bool force = false)
    {
        var entity = _context.Users.Find(userId);

        if (entity == null)
        {
            return NotFound;
        }

        var hasItems = _context.Items.Any(i => i.AssignedToId == userId);

        if (hasItems && !force)
        {
            return Conflict;
        }

        _context.Users.Remove(entity);
        _context.SaveChanges();

        return Deleted;
    }
}
