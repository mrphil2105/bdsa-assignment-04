using AutoMapper;

namespace Assignment.Infrastructure;


public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;
    private readonly IMapper _mapper;

    public TagRepository(KanbanContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var tagExist = _context.Tags.Any(t => t.Name == tag.Name);
        if(tagExist)
        {
            return (Conflict, 0);
        }

        var entity = _mapper.Map<Tag>(tag);

        _context.Tags.Add(entity);
        _context.SaveChanges();
        return (Created, entity.Id);
    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        return _context.Tags.Select(i => _mapper.Map<TagDTO>(i))
            .ToList();
    }

    public TagDTO? Find(int tagId)
    {
        return _mapper.Map<TagDTO>(_context.Tags.Find(tagId));
    }

    public Response Update(TagUpdateDTO tag)
    {
        var entity = Find(tag.Id);
        if(entity == null){
            return NotFound;
        }
        _context.SaveChanges();
        
        return Updated;
    }

    public Response Delete(int tagId, bool force = false)
    {
        throw new NotImplementedException();
    }
}
