using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
using static Tenor.Services.SharedService.ViewModels.SharedModels;

namespace Tenor.Services.SharedService
{

    public interface ISharedService
    {
        bool IsExist(int id, int? deviceId, string? kpiName, string? measureName, string? reportName);
        ResultWithMessage CheckValidFormat(OperationBinding input);
        bool IsFormatValid(OperationBinding input);
        bool AddExtraFields(int? kpiId, int? reportId, List<ExtraFieldValue> extFields);
        string ConvertListToString(string[]? input);
    }

    public class SharedService: ISharedService
    {
        private readonly TenorDbContext _db;
        private bool checkResult = true;

        public SharedService(TenorDbContext db)
        {
            _db = db;
        }

        public bool IsExist(int id, int? deviceId, string? kpiName, string? measureName, string? reportName)
        {
            bool isExist=false;
            if (!string.IsNullOrEmpty(kpiName))
            {
                isExist = _db.Kpis.Any(x => x.DeviceId == deviceId && x.Name == kpiName && x.Id != id);
                return isExist;
            }
            if (!string.IsNullOrEmpty(measureName))
            {
                isExist = _db.ReportMeasures.Any(x =>  x.DisplayName == measureName && x.Id != id);
                return isExist;
            }

            if (!string.IsNullOrEmpty(reportName))
            {
                isExist = _db.Reports.Any(x => x.DeviceId == deviceId && x.Name == reportName && x.Id != id);
                return isExist;
            }
            return isExist;


        }

        public ResultWithMessage CheckValidFormat(OperationBinding input)
        {

            if (input.Type.GetDisplayName() != "voidFunction")
            {
                return new ResultWithMessage(false, "KPI format is invalid");

            }
            if (input.Childs.Count == 0)
            {
                return new ResultWithMessage(false, "Please assign KPI formula");
            }
            if (input.Childs[0].Type.GetDisplayName() == "opt")
            {
                return new ResultWithMessage(false, "KPI format is invalid");

            }
            else
            {
                if (IsFormatValid(input))
                {
                    return new ResultWithMessage(true, null);
                }
            }

            return new ResultWithMessage(false, "KPI format is invalid");

        }

        public bool IsFormatValid(OperationBinding input)
        {

            try
            {
                if (input.Type.GetDisplayName() == "voidFunction")
                {
                    List<OperationBinding> data = input.Childs.ToList();
                    var levelType = data.Select(x => new { x.Type, x.Order, x.OperatorId, x.FunctionId }).ToList();
                    for (int i = 1; i < levelType.Count; i++)
                    {
                       

                        if (levelType[i].Type == levelType[i - 1].Type)
                        {
                            checkResult = false;
                            return checkResult;
                        }

                        if (levelType[i].Type.GetDisplayName() != "opt" && levelType[i - 1].Type.GetDisplayName() != "opt")
                        {
                            checkResult = false;
                            return checkResult;
                        }

                        if (levelType[levelType.Count() - 1].Type.GetDisplayName() == "opt")
                        {
                            checkResult = false;
                            return checkResult;
                        }
                    }

                }

                if (input.Type.GetDisplayName() == "function")
                {
                    Function func = _db.Functions.FirstOrDefault(x => x.Id == (int)input.FunctionId);
                    if (func.ArgumentsCount != input.Childs.Count())
                    {
                        checkResult = false;
                        return checkResult;

                    }
                }

                if (input.Childs != null && input.Childs.Count() > 0)
                {
                    foreach (var c in input.Childs)
                    {
                        IsFormatValid(c);

                    }
                }
                return checkResult;

            }
            catch (Exception ex)
            {
                checkResult = false;
                return checkResult;

            }

        }
        public bool AddExtraFields(int ? kpiId,int ? reportId, List<ExtraFieldValue> extFields)
        {
            foreach (var s in extFields)
            {
                if (kpiId != null)
                {
                    var extField = _db.KpiFields.AsNoTracking().Include(x => x.ExtraField).FirstOrDefault(x => x.Id == s.FieldId && x.IsActive);
                    if (extField != null)
                    {
                        if (extField.ExtraField.IsMandatory && string.IsNullOrEmpty(Convert.ToString(s.Value)))
                        {
                            return false;
                        }
                        if (extField.ExtraField.Type.GetDisplayName() == "List" || extField.ExtraField.Type.GetDisplayName() == "MultiSelectList")
                        {
                            string fileds = !string.IsNullOrEmpty(Convert.ToString(s.Value)) ? Convert.ToString(string.Join(",", s.Value)) : null;
                            string convertFields = fileds != null ? fileds.Replace("\",\"", ",").Replace("[\"", "").Replace("\"]", "") : null;
                            var ListValue = new KpiFieldValue((int)kpiId, s.FieldId, convertFields);
                            _db.KpiFieldValues.Add(ListValue);
                            _db.SaveChanges();

                        }
                        else
                        {
                            var StringValue = new KpiFieldValue((int)kpiId, s.FieldId, Convert.ToString(s.Value));
                            _db.KpiFieldValues.Add(StringValue);
                            _db.SaveChanges();

                        }

                    }
                }

                else
                {
                    var extField = _db.ReportFields.AsNoTracking().Include(x => x.ExtraField).FirstOrDefault(x => x.Id == s.FieldId && x.IsActive);
                    if (extField != null)
                    {
                        if (extField.ExtraField.IsMandatory && string.IsNullOrEmpty(Convert.ToString(s.Value)))
                        {
                            return false;
                        }
                        if (extField.ExtraField.Type.GetDisplayName() == "List" || extField.ExtraField.Type.GetDisplayName() == "MultiSelectList")
                        {
                            string fileds = !string.IsNullOrEmpty(Convert.ToString(s.Value)) ? Convert.ToString(string.Join(",", s.Value)) : null;
                            string convertFields = fileds != null ? fileds.Replace("\",\"", ",").Replace("[\"", "").Replace("\"]", "") : null;
                            var ListValue = new ReportFieldValue((int)reportId, s.FieldId, convertFields);
                            _db.ReportFieldValues.Add(ListValue);
                            _db.SaveChanges();

                        }
                        else
                        {
                            var StringValue = new ReportFieldValue((int)reportId, s.FieldId, Convert.ToString(s.Value));
                            _db.ReportFieldValues.Add(StringValue);
                            _db.SaveChanges();

                        }

                    }
                }
            }
            return true;
        }

        public string ConvertListToString(string[]? input)
        {
            string result = null;
            if(input is null || input.Count()==0)
            {
                return null;
            }

            result = string.Join(",", input);
            return result;
        }
    }
}
