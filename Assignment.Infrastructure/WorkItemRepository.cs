namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
        var tags = _context.Tags.Where(t => item.Tags.Contains(t.Name))
            .ToList();

        // Find tags that do not exist, yet, if any.
        var missingTags = item.Tags.Where(n => !tags.Exists(t => t.Name == n))
            .Select(n => new Tag(n));
        // Adding to the missing tags will save them to the database.
        // This is because so EF Core picks them up on the entity's list.
        tags.AddRange(missingTags);

        var entity = new WorkItem(item.Title)
        {
            AssignedToId = item.AssignedToId, Description = item.Description, Tags = tags
        };

        _context.Items.Add(entity);
        _context.SaveChanges();

        return (Created, entity.Id);
    }

    public WorkItemDetailsDTO Find(int itemId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        throw new NotImplementedException();
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
