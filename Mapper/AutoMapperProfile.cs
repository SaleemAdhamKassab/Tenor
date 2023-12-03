using AutoMapper;
using Microsoft.OpenApi.Extensions;
using Tenor.Dtos.AuthDto;
using Tenor.Dtos.KpiDto;
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

            CreateMap<KpiField, KpiExtraField>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ExtraField.Type.GetDisplayName()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExtraField.Name))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src=> ConvertContentType(src.ExtraField.Type.GetDisplayName(),src.ExtraField.Content)))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ExtraField.Url)).ReverseMap();
        }


        private dynamic ConvertContentType(string contenttype,string content)
        {
            if(contenttype != "List")
            {
                return content;
            }

            return content.Split(',').ToList();

        }
    }
}
