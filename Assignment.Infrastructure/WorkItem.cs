namespace Assignment.Infrastructure;

public class WorkItem
{
    public WorkItem(string title)
    {
        Title = title;
        Tags = new HashSet<Tag>();
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public int? AssignedToId { get; set; }

    public User? AssignedTo { get; set; }

    public string? Description { get; set; }

    public State State { get; set; }

    public ICollection<Tag> Tags { get; set; }
}
