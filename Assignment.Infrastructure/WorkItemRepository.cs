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
        var existingTags = _context.Tags.Where(t => item.Tags.Contains(t.Name))
            .ToList();
        // AutoMapper will create all tags regardless of their existence in the database.
        // This is a problem, because it will attempt to create tags with the same name.
        // So we remove the duplicate tags.
        entity.Tags = entity.Tags.Where(t => existingTags.All(et => et.Name != t.Name))
            .ToList();

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
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        throw new NotImplementedException();
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
