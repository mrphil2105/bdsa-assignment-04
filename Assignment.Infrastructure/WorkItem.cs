namespace Assignment.Infrastructure;

public class WorkItem
{
    private State _state;

    public WorkItem(string title)
    {
        Title = title;
        Created = DateTime.UtcNow;
        StateUpdated = DateTime.UtcNow;
        Tags = new HashSet<Tag>();
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public int? AssignedToId { get; set; }

    public User? AssignedTo { get; set; }

    public string? Description { get; set; }

    public DateTime Created { get; set; }

    public State State
    {
        get => _state;
        set
        {
            _state = value;
            StateUpdated = DateTime.UtcNow;
        }
    }

    public DateTime StateUpdated { get; set; }

    public ICollection<Tag> Tags { get; set; }
}
