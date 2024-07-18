using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using System.Text.RegularExpressions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Helper.Constant;
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
        List<OperationDto> GetSelfRelation(int optid);
        void deleteOperation(int id);
        string sqlBuild(int operationId);
        string getJoinKey(string tableName);
        IEnumerable<string> GetTablesNameRegExp(string query);
        string GetOracleQuery(string query, List<string> tablesName);
        dynamic ConvertContentType(string contenttype, string content);
        public List<ReportSubqueryModel> getOperationSubqueryModel(OperationBinding rootOperation);
        public List<ReportSubqueryModel> getOperationSubqueryModel(Operation rootOperation);
        public List<ReportSubqueryModel> getOperationSubqueryModel(OperationDto rootOperation);
    }

    public class SharedService: ISharedService
    {
        private readonly TenorDbContext _db;
        private bool checkResult = true;
        private readonly IMapper _mapper;
        private Operation prev = null;
        private Operation current = null;

        public SharedService(TenorDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
        public List<OperationDto> GetSelfRelation(int optid)
        {
            List<OperationDto> result = new List<OperationDto>();
            var opt = _db.Operations.Include(x => x.Function).Include(x => x.Counter).ThenInclude(x => x.Subset)
                .Include(x => x.Kpi).Include(x => x.Operator)
                .Include(x => x.Childs).FirstOrDefault(x => x.Id == optid);

            result.Add(_mapper.Map<OperationDto>(opt));
            if (opt.Childs.Count != 0)
            {
                foreach (var s in opt.Childs)
                {
                    GetSelfRelation(s.Id);
                }

            }

            return result;
        }
        public void deleteOperation(int id)
        {
            var operation = _db.Operations.Include(x => x.Childs).FirstOrDefault(x => x.Id == id);
            foreach (var childOp in operation.Childs)
            {
                deleteOperation(childOp.Id);
            }
            _db.Remove(operation);
        }
        public string sqlBuild(int operationId)
        {

            var sql = "";
            var operations = _db.Operations
                .Include(x => x.Childs)
                .Include(x => x.Counter)
                .ThenInclude(x => x.Subset)
                .Include(x => x.Kpi)
                .Include(x => x.Operator)
                .Include(x => x.Function)
                .Where(x => x.Id == operationId).ToList();
            foreach (var op in operations)
            {
                prev = current;
                current = op;
                switch (op.Type)
                {

                    case enOPerationTypes.opt:
                        sql += op.Operator.Name;
                        break;
                    case enOPerationTypes.number:
                        sql += op.Value;
                        break;
                    case enOPerationTypes.counter:
                        sql += $"{op.Aggregation}({op.Counter.Subset.TableName}.{op.Counter.ColumnName})";
                        break;
                    case enOPerationTypes.kpi:
                        sql += $"({sqlBuild(op.Kpi.OperationId)})";
                        break;
                    case enOPerationTypes.voidFunction:
                        if ((prev != null && prev.Operator != null) ? prev.Operator.Name == "/" : false)
                            sql += $"NoZero({string.Join(" ", op.Childs.Select(x => sqlBuild(x.Id)).ToArray())})";
                        else
                            sql += $"({string.Join(" ", op.Childs.Select(x => sqlBuild(x.Id)).ToArray())})";
                        break;
                    case enOPerationTypes.function:
                        {
                            if (op.Function.Name.ToLower() == "if")
                            {
                                sql += $"(CASE WHEN ({sqlBuild(op.Childs.ToArray()[0].Id)}) THEN ({sqlBuild(op.Childs.ToArray()[1].Id)}) ELSE ({sqlBuild(op.Childs.ToArray()[2].Id)}) END)";
                            }
                            else
                            {
                                sql += $"{op.Function.Name}({string.Join(", ", op.Childs.Select(x => sqlBuild(x.Id)).ToArray())})";
                            }
                            break;
                        }
                    default:
                        sql += " ";
                        break;
                }
                sql += " ";
            }
            return sql;

        }
        public string getJoinKey(string tableName)
        {
            var subset = _db.Subsets.FirstOrDefault(x => x.TableName.ToLower() == tableName.ToLower());
            if (subset is null)
            {
                return null;
            }
            return subset.FactDimensionReference;
        }
        public IEnumerable<string> GetTablesNameRegExp(string query)
        {
            List<string> tablesName = new List<string>();
            string pattern = @"\b[tech4_]\w+";
            Regex rg = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matchedTables = rg.Matches(query);
            for (int i = 0; i < matchedTables.Count; i++)
            {
                if ((matchedTables[i].Value.ToLower().Contains("tech")))
                {
                    tablesName.Add(matchedTables[i].Value);

                }
            }
            return tablesName.Distinct();
        }
        public string GetOracleQuery(string query, List<string> tablesName)
        {
            query = "select TO_CHAR(" + query + ") ";
            string formExp = "";
            string joinExp = "";
            string whereExp = "";
            string joinKey = getJoinKey(tablesName[0]);
            string lastDate = DateTime.Now.Year.ToString() +
                              DateTime.Now.ToString("MM") +
                           Convert.ToInt32(Convert.ToString(Convert.ToInt32(DateTime.Now.ToString("dd")) - 1)).ToString("00");

            formExp += " from " + tablesName[0] + " " + tablesName[0];

            if (tablesName.Count() > 1)
            {
                for (int i = 0; i <= tablesName.Count() - 1; i++)
                {
                    if (i > 0)
                    {
                        joinExp += " join " + tablesName[i] + " " + tablesName[i] + " on " + tablesName[i] + "." + joinKey + "=" +
                            tablesName[i - 1] + "." + joinKey + " and " + tablesName[i] + ".c_date = " + tablesName[i - 1] + ".c_date";
                    }
                }
            }
            if (tablesName[0].ToLower().Contains("_details"))
            {
                whereExp += " where " + tablesName[0] + ".c_date between " + lastDate + "0000" + " and " + lastDate + "0159";

            }
            else
            {
                whereExp += " where " + tablesName[0] + ".c_date between " + lastDate + "00" + " and " + lastDate + "01";

            }
            query += formExp + joinExp + whereExp;
            return query;




        }
        public dynamic ConvertContentType(string contenttype, string content)
        {
            if ((contenttype == "List" && !string.IsNullOrEmpty(content) ? !content.Contains(",") : true) && contenttype != "MultiSelectList")
            {
                return content;
            }

            return !string.IsNullOrEmpty(content) ? content.Split(',').ToList() : null;

        }
        public List<ReportSubqueryModel> getOperationSubqueryModel(OperationBinding rootOperation)
        {
            List<ReportSubqueryModel> reportSubqueries = [];
            foreach (OperationBinding operation in rootOperation.Childs)
            {
                if (operation.Type == enOPerationTypes.counter)
                {
                    var counter = _db.Counters.Include(x => x.Subset).FirstOrDefault(x => x.Id == operation.CounterId);
                    if (counter == null)
                    {
                        continue;
                    }
                    var existCounterAndAggregation = reportSubqueries
                        .Any(x => x.ReportSubqueryMeasures
                        .Any(y => y.Aggregation == operation.Aggregation.GetDisplayName() && y.ColumnName == counter.ColumnName));
                    if (!existCounterAndAggregation)
                    {
                        var existDeviceAndSubset = reportSubqueries.FirstOrDefault(x => x.SubsetTableName == counter.Subset.TableName && x.DeviceId == counter.Subset.DeviceId);
                        if (existDeviceAndSubset != null)
                        {
                            existDeviceAndSubset.ReportSubqueryMeasures.Add(new ReportSubqueryMeasure
                            {
                                CounterId = operation.CounterId ?? 0,
                                Aggregation = operation.Aggregation.GetDisplayName(),
                                ColumnName = counter.ColumnName
                            });
                        }
                        else
                        {
                            reportSubqueries.Add(new ReportSubqueryModel
                            {
                                DeviceId = counter.Subset.DeviceId,
                                SubsetTableName = counter.Subset.TableName,
                                ReportSubqueryMeasures = new List<ReportSubqueryMeasure>
                                {
                                    new ReportSubqueryMeasure
                                    {
                                        CounterId = operation.CounterId ?? 0,
                                        Aggregation = operation.Aggregation.GetDisplayName(),
                                        ColumnName = counter.ColumnName
                                    }
                                }
                            });
                        }
                    }
                }
                else if (operation.Type == enOPerationTypes.voidFunction ||
                        operation.Type == enOPerationTypes.function)
                {
                    foreach (var item in operation.Childs ?? [])
                    {
                        reportSubqueries.AddRange(getOperationSubqueryModel(item));
                    }
                }
                else if (operation.Type == enOPerationTypes.kpi)
                {
                    var item = _db.Kpis.Include(x => x.Operation).FirstOrDefault(x => x.Id == operation.KpiId);
                    if (item != null)
                    {
                        reportSubqueries.AddRange(getOperationSubqueryModel(item.Operation));
                    }
                }
            }

            return reportSubqueries;
        }
        public List<ReportSubqueryModel> getOperationSubqueryModel(OperationDto rootOperation)
        {
            List<ReportSubqueryModel> reportSubqueries = [];
            foreach (OperationDto operation in rootOperation.Childs)
            {
                if (operation.Type == enOPerationTypes.counter)
                {
                    var counter = _db.Counters.Include(x => x.Subset).FirstOrDefault(x => x.Id == operation.CounterId);
                    if (counter == null)
                    {
                        continue;
                    }
                    var existCounterAndAggregation = reportSubqueries
                        .Any(x => x.ReportSubqueryMeasures
                        .Any(y => y.Aggregation == operation.Aggregation.GetDisplayName() && y.ColumnName == counter.ColumnName));
                    if (!existCounterAndAggregation)
                    {
                        var existDeviceAndSubset = reportSubqueries.FirstOrDefault(x => x.SubsetTableName == counter.Subset.TableName && x.DeviceId == counter.Subset.DeviceId);
                        if (existDeviceAndSubset != null)
                        {
                            existDeviceAndSubset.ReportSubqueryMeasures.Add(new ReportSubqueryMeasure
                            {
                                CounterId = operation.CounterId ?? 0,
                                Aggregation = operation.Aggregation.GetDisplayName(),
                                ColumnName = counter.ColumnName
                            });
                        }
                        else
                        {
                            reportSubqueries.Add(new ReportSubqueryModel
                            {
                                DeviceId = counter.Subset.DeviceId,
                                SubsetTableName = counter.Subset.TableName,
                                ReportSubqueryMeasures = new List<ReportSubqueryMeasure>
                                {
                                    new ReportSubqueryMeasure
                                    {
                                        CounterId = operation.CounterId ?? 0,
                                        Aggregation = operation.Aggregation.GetDisplayName(),
                                        ColumnName = counter.ColumnName
                                    }
                                }
                            });
                        }
                    }
                }
                else if (operation.Type == enOPerationTypes.voidFunction ||
                        operation.Type == enOPerationTypes.function)
                {
                        reportSubqueries.AddRange(getOperationSubqueryModel(operation));   
                }
                else if (operation.Type == enOPerationTypes.kpi)
                {
                    var item = _db.Kpis.Include(x => x.Operation).FirstOrDefault(x => x.Id == operation.KpiId);
                    if (item != null)
                    {
                        reportSubqueries.AddRange(getOperationSubqueryModel(item.Operation));
                    }
                }
            }

            return reportSubqueries;
        }
        public List<ReportSubqueryModel> getOperationSubqueryModel(Operation rootOperation)
        {
            List<ReportSubqueryModel> reportSubqueries = [];
            var operationChilds = _db.Operations.Where(x => x.ParentId == rootOperation.Id);
            foreach (var operation in operationChilds.ToList() ?? [])
            {
                if (operation.Type == enOPerationTypes.counter)
                {
                    var counter = _db.Counters.Include(x => x.Subset).FirstOrDefault(x => x.Id == operation.CounterId);
                    if (counter == null)
                    {
                        continue;
                    }
                    var existCounterAndAggregation = reportSubqueries
                        .Any(x => x.ReportSubqueryMeasures
                        .Any(y => y.Aggregation == operation.Aggregation.GetDisplayName() && y.ColumnName == counter.ColumnName));
                    if (!existCounterAndAggregation)
                    {
                        var existDeviceAndSubset = reportSubqueries.FirstOrDefault(x => x.SubsetTableName == counter.Subset.TableName && x.DeviceId == counter.Subset.DeviceId);
                        if (existDeviceAndSubset != null)
                        {
                            existDeviceAndSubset.ReportSubqueryMeasures.Add(new ReportSubqueryMeasure
                            {
                                CounterId = operation.CounterId ?? 0,
                                Aggregation = operation.Aggregation.GetDisplayName(),
                                ColumnName = counter.ColumnName
                            });
                        }
                        else
                        {
                            reportSubqueries.Add(new ReportSubqueryModel
                            {
                                DeviceId = counter.Subset.DeviceId,
                                SubsetTableName = counter.Subset.TableName,
                                ReportSubqueryMeasures = new List<ReportSubqueryMeasure>
                                {
                                    new ReportSubqueryMeasure
                                    {
                                        CounterId = operation.CounterId ?? 0,
                                        Aggregation = operation.Aggregation.GetDisplayName(),
                                        ColumnName = counter.ColumnName
                                    }
                                }
                            });
                        }
                    }
                }
                else if (operation.Type == enOPerationTypes.voidFunction ||
                        operation.Type == enOPerationTypes.function)
                {
                    
                        reportSubqueries.AddRange(getOperationSubqueryModel(operation));
                    
                }
                else if (operation.Type == enOPerationTypes.kpi)
                {
                    var item = _db.Kpis.Include(x => x.Operation).FirstOrDefault(x => x.Id == operation.KpiId);
                    if (item != null)
                    {
                        reportSubqueries.AddRange(getOperationSubqueryModel(item.Operation));
                    }
                }
            }
            return reportSubqueries;
        }


    }
}
