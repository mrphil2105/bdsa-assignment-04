using System.Linq.Expressions;
using AutoMapper;

namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;
    private readonly IMapper _mapper;

    public WorkItemRepository(KanbanContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
        if (item.AssignedToId.HasValue)
        {
            var userExists = _context.Users.Any(u => u.Id == item.AssignedToId);

            if (!userExists)
            {
                return (BadRequest, 0);
            }
        }

        var entity = _mapper.Map<WorkItem>(item);

        if (!TryUpdateTags(item.Tags, entity))
        {
            return (BadRequest, 0);
        }

        _context.Items.Add(entity);
        _context.SaveChanges();

        return (Created, entity.Id);
    }

    public WorkItemDetailsDTO? Find(int itemId)
    {
        return _mapper.Map<WorkItemDetailsDTO>(FindEntity(itemId));
    }

    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        return _context.Items.Include(i => i.AssignedTo)
            .Include(i => i.Tags)
            .Select(i => _mapper.Map<WorkItemDTO>(i))
            .ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        return ReadByState(Removed);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        return ReadWithFilter(i => i.Tags.Any(t => t.Name == tag));
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        return ReadWithFilter(i => i.AssignedToId == userId);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        return ReadWithFilter(i => i.State == state);
    }

    public Response Update(WorkItemUpdateDTO item)
    {
        var entity = FindEntity(item.Id);

        if (entity == null)
        {
            return NotFound;
        }

        if (item.AssignedToId.HasValue && item.AssignedToId != entity.AssignedToId)
        {
            var userExists = _context.Users.Any(u => u.Id == item.AssignedToId);

            if (!userExists)
            {
                return BadRequest;
            }
        }

        _mapper.Map(item, entity);

        if (!TryUpdateTags(item.Tags, entity))
        {
            return BadRequest;
        }

        _context.SaveChanges();

        return Updated;
    }

    public Response Delete(int itemId)
    {
        var entity = _context.Items.Find(itemId);

        if (entity == null)
        {
            return NotFound;
        }

        switch (entity.State)
        {
            case New:
                _context.Items.Remove(entity);
                _context.SaveChanges();

                return Deleted;
            case Active:
                entity.State = Removed;
                _context.SaveChanges();

                return Updated;
            default:
                return Conflict;
        }
    }

    private bool TryUpdateTags(ICollection<string> tags, WorkItem entity)
    {
        var distinctCount = tags.Distinct()
            .Count();

        // Check for duplicate tags.
        if (distinctCount < tags.Count)
        {
            return false;
        }

        var existingTags = _context.Tags.Where(t => tags.Contains(t.Name));

        // AutoMapper will create all tags regardless of their existence in the database.
        // This is a problem, because it will attempt to create tags with the same name.
        // So, make sure the existing tags in the database are used.
        foreach (var existingTag in existingTags)
        {
            var tag = entity.Tags.Single(t => t.Name == existingTag.Name);

            // We can't update the id of 'tag', as EF Core already tracks 'existingTag' with that id.
            entity.Tags.Remove(tag);
            entity.Tags.Add(existingTag);
        }

        return true;
    }

    private WorkItem? FindEntity(int id)
    {
        return _context.Items.Include(i => i.AssignedTo)
            .Include(i => i.Tags)
            .SingleOrDefault(i => i.Id == id);
    }

    private IReadOnlyCollection<WorkItemDTO> ReadWithFilter(Expression<Func<WorkItem, bool>> predicate)
    {
        return _context.Items.Include(i => i.AssignedTo)
            .Include(i => i.Tags)
            .Where(predicate)
            .Select(i => _mapper.Map<WorkItemDTO>(i))
            .ToList();
    }
}
