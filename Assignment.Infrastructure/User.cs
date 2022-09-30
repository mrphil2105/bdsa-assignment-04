namespace Assignment.Infrastructure;

public class User
{
    public User(string name, string email)
    {
        Name = name;
        Email = email;
        Items = new HashSet<WorkItem>();
    }

    public int Id { get; set; }

    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public ICollection<WorkItem> Items { get; set; }
}
