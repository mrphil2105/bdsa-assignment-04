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

        CreateMap<WorkItem, WorkItemDetailsDTO>()
            .MapRecordMember(d => d.AssignedToName, i => i.AssignedTo == null ? null : i.AssignedTo.Name)
            .MapRecordMember(d => d.Tags, i => i.Tags.Select(t => t.Name));

        CreateMap<WorkItem, WorkItemDTO>()
            .MapRecordMember(d => d.AssignedToName, i => i.AssignedTo == null ? null : i.AssignedTo.Name)
            .MapRecordMember(d => d.Tags, i => i.Tags.Select(t => t.Name));

        CreateMap<WorkItemUpdateDTO, WorkItem>()
            .ForMember(i => i.AssignedTo, o => o.Ignore())
            .ForMember(i => i.Created, o => o.Ignore())
            .ForMember(i => i.StateUpdated, o => o.Ignore());

        CreateMap<UserCreateDTO, User>()
            .ForMember(u => u.Id, o => o.Ignore())
            .ForMember(u => u.Items, o => o.Ignore());

        CreateMap<User, UserDTO>();
    }
}
