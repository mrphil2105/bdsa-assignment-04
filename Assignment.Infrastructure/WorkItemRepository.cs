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
        var entity = _mapper.Map<WorkItem>(item);
        var existingTags = _context.Tags.Where(t => item.Tags.Contains(t.Name));

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

        _context.Items.Add(entity);
        _context.SaveChanges();

        return (Created, entity.Id);
    }

    public WorkItemDetailsDTO? Find(int itemId)
    {
        var entity = _context.Items.Include(i => i.AssignedTo)
            .Include(i => i.Tags)
            .SingleOrDefault(i => i.Id == itemId);

        return _mapper.Map<WorkItemDetailsDTO>(entity);
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
        return _context.Items.Include(i => i.AssignedTo)
            .Include(i => i.Tags)
            .Where(i => i.Tags.Any(t => t.Name == tag))
            .Select(i => _mapper.Map<WorkItemDTO>(i))
            .ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        return _context.Items.Include(i => i.AssignedTo)
            .Include(i => i.Tags)
            .Where(i => i.AssignedToId == userId)
            .Select(i => _mapper.Map<WorkItemDTO>(i))
            .ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        return _context.Items.Include(i => i.AssignedTo)
            .Include(i => i.Tags)
            .Where(i => i.State == state)
            .Select(i => _mapper.Map<WorkItemDTO>(i))
            .ToList();
    }

    public Response Update(WorkItemUpdateDTO item)
    {
        throw new NotImplementedException();
    }

    public Response Delete(int itemId)
    {
        throw new NotImplementedException();
    }
}
