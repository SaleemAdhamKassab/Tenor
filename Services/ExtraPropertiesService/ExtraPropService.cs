using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using System.Linq;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

public interface IExtraPropService
{
    ResultWithMessage add(CreateExtraFieldViewModel input);
    ResultWithMessage edit(int id, CreateExtraFieldViewModel input);
    ResultWithMessage delete(int id);
    ResultWithMessage GetAll(ExtraFieldFilter input);
    ResultWithMessage GetById(int id);


}

public class ExtraPropService : IExtraPropService
{
    private readonly TenorDbContext _db;
    private readonly IMapper _mapper;
    public ExtraPropService(TenorDbContext tenorDbContext, IMapper mapper)
    {
        _db = tenorDbContext;
        _mapper = mapper;

    }


    public ResultWithMessage add(CreateExtraFieldViewModel input)
    {
        ExtraField extraField = new ExtraField(input);
        if(input.IsForKpi)
        {
            extraField.addKpiField(extraField);
        }
        if (input.IsForReport)
        {
            extraField.addReportField(extraField);
        }
        if (input.IsForDashboard)
        {
            extraField.addDashboardField(extraField);
        }
        _db.ExtraFields.Add(extraField);
        _db.SaveChanges();
        //----------------------------------------
        var result = new ExtraFieldViewModel()
        {
            Id = extraField.Id,
            Name = extraField.Name,
            Type = extraField.Type,
            TypeName = extraField.Type.GetDisplayName(),
            Url = extraField.Url,
            Content = extraField.Content,
            IsForKpi = input.IsForKpi,
            IsForReport = input.IsForReport,
            IsForDashboard = input.IsForDashboard,
            IsMandatory = input.IsMandatory,
            DeviceId=extraField.DeviceId,
            DeviceName= extraField.Device!=null? extraField.Device.Name : null,
        };
        return new ResultWithMessage(result, null);
    }

    public ResultWithMessage delete(int id)
    {
        ExtraField extField = _db.ExtraFields.FirstOrDefault(x => x.Id == id);
        if (extField is null)
        {
            return new ResultWithMessage(null, "Extra field is invalid");

        }
        _db.ExtraFields.Remove(extField);
        _db.SaveChanges();
        return new ResultWithMessage(true,null);
    }

    public ResultWithMessage edit(int id, CreateExtraFieldViewModel input)
    {
        if(id!=input.Id)
        {
            return new ResultWithMessage(null, "ID Mismatch");
        }
        ExtraField extField = _db.ExtraFields.Include(x=>x.Device).Include(x=>x.KpiFields)
        .Include(x=>x.ReportFields)
        .Include(x=>x.DashboardFields).AsNoTracking().FirstOrDefault(x => x.Id == id);

        if (extField is null)
        {
            return new ResultWithMessage(null, "Extra field is invalid");

        }
        //-----------------------------------------------
        extField.Name = input.Name;
        extField.Content = input.Content;
        extField.Url = input.Url;
        extField.Type = input.Type;
        extField.IsMandatory = input.IsMandatory;
        extField.DeviceId = input.DeviceId;

        if ((extField.KpiFields!=null  && extField.KpiFields.Count != 0) && !input.IsForKpi)
        {
            _db.KpiFields.Entry(extField.KpiFields.SingleOrDefault()).State = EntityState.Deleted;
        }
        if ((extField.ReportFields != null && extField.ReportFields.Count!=0) && !input.IsForReport)
        {
            _db.ReportFields.Entry(extField.ReportFields.SingleOrDefault()).State = EntityState.Deleted;

        }
        if ((extField.DashboardFields != null && extField.DashboardFields.Count != 0) && !input.IsForDashboard)
        {
            _db.DashboardFields.Entry(extField.DashboardFields.SingleOrDefault()).State = EntityState.Deleted;

        }
        //--------------------------------------------------------------------
        if ((extField.KpiFields is null || extField.KpiFields.Count==0) && input.IsForKpi)
        {
            extField.addKpiField(extField);
        }
        if ((extField.ReportFields is null || extField.ReportFields.Count==0) && input.IsForReport)
        {
            extField.addReportField(extField);
        }
        if ((extField.DashboardFields is null || extField.DashboardFields.Count==0) && input.IsForDashboard)
        {
            extField.addDashboardField(extField);
        }
        _db.ExtraFields.Update(extField);
        _db.SaveChanges();
        //-------------------------------------
        var result = new ExtraFieldViewModel()
        {
            Id = extField.Id,
            Name = extField.Name,
            Type = extField.Type,
            TypeName = extField.Type.GetDisplayName(),
            Url = extField.Url,
            Content = extField.Content,
            IsForKpi = input.IsForKpi,
            IsForReport = input.IsForReport,
            IsForDashboard = input.IsForDashboard,
            IsMandatory = input.IsMandatory,
            DeviceId=input.DeviceId,
            DeviceName = extField.Device != null ? extField.Device.Name : null,

        };
        return new ResultWithMessage(result, null);
    }

    public ResultWithMessage GetAll(ExtraFieldFilter input)
    {
        try
        {
            //------------------------------Data source------------------------------------------------
            IQueryable<ExtraField> query = _db.ExtraFields.AsQueryable();
            var extraIds = query.Select(x => x.Id).AsQueryable();
            //------------------------------Data filter-----------------------------------------------------------

            if (!string.IsNullOrEmpty(input.SearchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(input.SearchQuery.ToLower()));

            }

            if (input.IsKpi != null)
            {
                
                query = query.Where(x => x.KpiFields.Any(y => extraIds.Contains(y.FieldId)) == input.IsKpi);
            }

            if (input.IsReport != null)
            {
                query = query.Where(x => x.ReportFields.Any(y => extraIds.Contains(y.FieldId)) == input.IsReport);
            }

            if (input.IsDashboard != null)
            {
                query = query.Where(x => x.DashboardFields.Any(y => extraIds.Contains(y.FieldId)) == input.IsDashboard);
            }

            if (input.DeviceId != null && input.DeviceId!=0)
            {
                query = query.Where(x => x.DeviceId==input.DeviceId);
            }

            //mapping wit DTO querable
            var queryViewModel = query.Select(x => new ExtraFieldViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                TypeName = x.Type.GetDisplayName(),
                Url = x.Url,
                Content = x.Content,
                IsForKpi = x.KpiFields.Any(y => query.Select(x => x.Id).Contains(y.FieldId)),
                IsForReport = x.ReportFields.Any(y => query.Select(x => x.Id).Contains(y.FieldId)),
                IsForDashboard = x.DashboardFields.Any(y => query.Select(x => x.Id).Contains(y.FieldId)),
                IsMandatory = x.IsMandatory,
                DeviceId=x.DeviceId,
                DeviceName = x.Device != null ? x.Device.Name : null,

            });
            //Sort and paginition
            return sortAndPagination(input, queryViewModel);

        }
        catch (Exception ex)
        {
            return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

        }
    
    }

    public ResultWithMessage GetById(int id)
    {
        IQueryable<ExtraField> query = _db.ExtraFields.Where(x=>x.Id==id).AsQueryable();
        var extraIds = query.Select(x => x.Id).AsQueryable();
        if(query.Count()==0 || query is null)
        {
            return new ResultWithMessage(null,"This ID is invalid"); 
        }
        var queryViewModel = query.Select(x => new ExtraFieldViewModel()
        {
            Id = x.Id,
            Name = x.Name,
            Type = x.Type,
            TypeName = x.Type.GetDisplayName(),
            Url = x.Url,
            Content = x.Content,
            IsForKpi = x.KpiFields.Any(y => query.Select(x => x.Id).Contains(y.FieldId)),
            IsForReport = x.ReportFields.Any(y => query.Select(x => x.Id).Contains(y.FieldId)),
            IsForDashboard = x.DashboardFields.Any(y => query.Select(x => x.Id).Contains(y.FieldId)),
            IsMandatory = x.IsMandatory,
            DeviceId=x.DeviceId,
            DeviceName = x.Device != null ? x.Device.Name : null,

        }).FirstOrDefault();

        return new ResultWithMessage(queryViewModel,null);
    }

    private ResultWithMessage sortAndPagination(ExtraFieldFilter extraFilterModel, IQueryable<ExtraFieldViewModel> queryViewModel)
    {
        if (!string.IsNullOrEmpty(extraFilterModel.SortActive))
        {

            var sortProperty = typeof(ExtraFieldViewModel).GetProperty(char.ToUpper(extraFilterModel.SortActive[0]) + extraFilterModel.SortActive.Substring(1));
            if (sortProperty != null && extraFilterModel.SortDirection == "asc")
                queryViewModel = queryViewModel.OrderBy2(extraFilterModel.SortActive);

            else if (sortProperty != null && extraFilterModel.SortDirection == "desc")
                queryViewModel = queryViewModel.OrderByDescending2(extraFilterModel.SortActive);

            int Count = queryViewModel.Count();

            var result = queryViewModel.Skip((extraFilterModel.PageIndex) * extraFilterModel.PageSize)
            .Take(extraFilterModel.PageSize).ToList();


            return new ResultWithMessage(new DataWithSize(Count, result), "");
        }

        else
        {
            int Count = queryViewModel.Count();
            var result = queryViewModel.Skip((extraFilterModel.PageIndex) * extraFilterModel.PageSize)
            .Take(extraFilterModel.PageSize).ToList();

            return new ResultWithMessage(new DataWithSize(Count, result), "");
        }

    }
}