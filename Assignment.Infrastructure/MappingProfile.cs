using AutoMapper;

namespace Assignment.Infrastructure;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WorkItemCreateDTO, WorkItem>()
            .ForMember(i => i.Id, o => o.Ignore())
            .ForMember(i => i.AssignedTo, o => o.Ignore())
            .ForMember(i => i.Created, o => o.Ignore())
            .ForMember(i => i.State, o => o.Ignore())
            .ForMember(i => i.StateUpdated, o => o.Ignore());
    }
}
