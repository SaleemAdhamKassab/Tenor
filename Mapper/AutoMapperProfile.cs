using AutoMapper;
using Microsoft.OpenApi.Extensions;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using Tenor.Services.DevicesService.ViewModels;
using Tenor.Services.SubsetsService.ViewModels;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //-----------------------------AUTH-------------------------------------------
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




            //-----------------------------KPI--------------------------------------
            CreateMap<Operation, OperationDto>()
                .ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.Operator != null ? src.Operator.Name : null))
                .ForMember(dest => dest.FunctionName, opt => opt.MapFrom(src => src.Function != null ? src.Function.Name : null))
                .ForMember(dest => dest.KpiName, opt => opt.MapFrom(src => src.Kpi != null ? src.Kpi.Name : null))
                .ForMember(dest => dest.CounterName, opt => opt.MapFrom(src => src.Counter != null ? src.Counter.Name : null))
                .ForMember(dest => dest.SubsetId, opt => opt.MapFrom(src => src.Counter != null ? src.Counter.Subset.Id : 0))
                .ForMember(dest => dest.SubsetName, opt => opt.MapFrom(src => src.Counter != null ? src.Counter.Subset.Name : null))
                .ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.Counter != null ? src.Counter.Subset.TableName : null))
                .ForMember(dest => dest.ColumnName, opt => opt.MapFrom(src => src.Counter != null ? src.Counter.ColumnName : null)).ReverseMap();

            CreateMap<Operation, OperationBinding>().ReverseMap();
            CreateMap<Kpi, CreateKpi>().ReverseMap();
            CreateMap<KpiField, KpiExtraField>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ExtraField.Type.GetDisplayName()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExtraField.Name))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => ConvertContentType(src.ExtraField.Type.GetDisplayName(), src.ExtraField.Content)))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ExtraField.Url)).ReverseMap();

            CreateMap<KpiFieldValue, KpiFieldValueViewModel>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.FieldId, opt => opt.MapFrom(src => src.KpiFieldId))
                 .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.KpiField.ExtraField.Name))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.KpiField.ExtraField.Type))
                 .ForMember(dest => dest.Value, opt => opt.MapFrom(src => ConvertContentType(src.KpiField.ExtraField.Type.GetDisplayName(), src.FieldValue)))
                 .ReverseMap();


            CreateMap<Kpi, KpiViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ExtraFields, opt => opt.MapFrom(src => src.KpiFieldValues))
                .ForMember(dest => dest.Operations, opt => opt.MapFrom(src => src.Operation))
                .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.Device.Name))
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.ModifyBy, opt => opt.MapFrom(src => src.ModifyBy))
                .ForMember(dest => dest.ModifyDate, opt => opt.MapFrom(src => src.ModifyDate))
                .ReverseMap();


            //-----------------------------Counter--------------------------------------
            CreateMap<CounterField, CounterExtraField>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ExtraField.Type.GetDisplayName()))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExtraField.Name))
               .ForMember(dest => dest.Content, opt => opt.MapFrom(src => ConvertContentType(src.ExtraField.Type.GetDisplayName(), src.ExtraField.Content)))
               .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ExtraField.Url)).ReverseMap();

            CreateMap<CounterFieldValue, CounterExtraFieldValueViewModel>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.FieldId, opt => opt.MapFrom(src => src.CounterFieldId))
                 .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.CounterField.ExtraField.Name))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.CounterField.ExtraField.Type))
                 .ForMember(dest => dest.Value, opt => opt.MapFrom(src => ConvertContentType(src.CounterField.ExtraField.Type.GetDisplayName(), src.FieldValue)))
                 .ReverseMap();

            //-----------------------------Subset--------------------------------------
            CreateMap<SubsetField, SubsetExtraField>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ExtraField.Type.GetDisplayName()))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExtraField.Name))
                 .ForMember(dest => dest.Content, opt => opt.MapFrom(src => ConvertContentType(src.ExtraField.Type.GetDisplayName(), src.ExtraField.Content)))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ExtraField.Url)).ReverseMap();

            CreateMap<SubsetFieldValue, SubsetExtraFieldValueViewModel>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.FieldId, opt => opt.MapFrom(src => src.SubsetFieldId))
                 .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.SubsetField.ExtraField.Name))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.SubsetField.ExtraField.Type))
                 .ForMember(dest => dest.Value, opt => opt.MapFrom(src => ConvertContentType(src.SubsetField.ExtraField.Type.GetDisplayName(), src.FieldValue)))
                 .ReverseMap();


           

        }


        private dynamic ConvertContentType(string contenttype, string content)
        {
            if ((contenttype == "List" && !string.IsNullOrEmpty(content)? !content.Contains(","):true ) && contenttype != "MultiSelectList")
            {
                return content;
            }

            return content.Split(',').ToList();

        }
    }
}
