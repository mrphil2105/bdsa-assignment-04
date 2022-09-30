namespace Assignment.Infrastructure;

public class Tag
{
    public Tag(string name)
    {
        Name = name;
        WorkItems = new HashSet<WorkItem>();
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<WorkItem> WorkItems { get; set; }
}
