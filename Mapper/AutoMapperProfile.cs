using AutoMapper;
using Tenor.Dtos;
using Tenor.Dtos.AuthDto;
using Tenor.Models;

namespace Tenor.Mapper
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<UserTenantRole, UserTenantDto>()
                .ForMember(dest => dest.userName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
                .ReverseMap();

            CreateMap<GroupTenantRole, GroupTenantDto>()
                .ForMember(dest => dest.groupName, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
                .ReverseMap();

            CreateMap<Operation, OperationDto>().ReverseMap();
            CreateMap<Kpi, CreateKpi>().ReverseMap();
            CreateMap<Kpi, UpdateKpi>().ReverseMap();

        }
    }
}
