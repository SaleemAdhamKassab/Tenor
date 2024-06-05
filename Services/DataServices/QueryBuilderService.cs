using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.OpenApi.Extensions;
using System.Reflection;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services.SharedService;
using static Tenor.Helper.Constant;
using static Tenor.Services.ReportService.ViewModels.ReportModels;
using static Tenor.Services.SharedService.ViewModels.SharedModels;

namespace Tenor.Services.DataServices
{
    public interface IQueryBuilderService
    {
        public string getFilterOptionsQuery(int levelId, string? searchQuery, int pageIndex, int pageSize);
        public string getReportQueryByCreateReport(CreateReport report);
    }
    public class QueryBuilderService : IQueryBuilderService
    {
        private readonly TenorDbContext _db;
        private readonly ISharedService _sharedService;
        public QueryBuilderService(TenorDbContext db, ISharedService sharedService)
        {
            _db = db;
            _sharedService = sharedService;
        }

        public string getFilterOptionsQuery(int levelId, string? searchQuery, int pageIndex, int pageSize)
        {
            var queryObj = _db.Levels.Where(x => x.Id == levelId).Select(x => x.DimensionLevels.Select(y => new
            {
                y.ColumnName,
                y.OrderBy,
                y.Dimension.TableName
            })).SelectMany(x => x).FirstOrDefault();
            var filterOptionsQuery = "SELECT DISTINCT"
                                    + $" {queryObj.ColumnName}"
                                    + $" FROM {queryObj.TableName}"
                                    + (searchQuery != null ? $" WHERE LOWER({queryObj.ColumnName}) LIKE '{searchQuery.ToLower()}%'" : " ")
                                     + $" ORDER BY {queryObj.OrderBy}"
                                     + $" OFFSET {pageIndex * pageSize}"
                                     + $" ROWS FETCH NEXT {pageSize} ROWS ONLY";

            return filterOptionsQuery.ToString();
        }
        public string getReportQueryByCreateReport(CreateReport report)
        {
            var sql = "";

            var allSubqueryModels = report.Measures.Select(x => _sharedService.getOperationSubqueryModel(x.Operation)).SelectMany(x => x).ToList();
            var joinedSubQueryModel = new List<ReportSubqueryModel>
            {
                allSubqueryModels[0]
            };
            for (int i = 1; i < (allSubqueryModels.Count); i++)
            {
                var exist = joinedSubQueryModel.FirstOrDefault(x => x.DeviceId == allSubqueryModels[i].DeviceId && x.SubsetTableName == allSubqueryModels[i].SubsetTableName);
                if (exist != null)
                {
                    exist.ReportSubqueryMeasures.AddRange(allSubqueryModels[i].ReportSubqueryMeasures);
                    exist.ReportSubqueryMeasures = exist.ReportSubqueryMeasures.GroupBy(c => new { c.ColumnName, c.Aggregation }).Select(g => g.First()).ToList();
                }
                else
                {
                    joinedSubQueryModel.Add(allSubqueryModels[i]);
                }
            }

            var reportLevelIds = report.Levels.Select(x => x.LevelId);
            var reportFilterLevelIds = report.ContainerOfFilters.Select(cf => cf.ReportFilters).SelectMany(f => f).Select(f => f.LevelId);
            for (int i = 0; i < joinedSubQueryModel.Count; i++)
            {
                var subquery = joinedSubQueryModel[i];
                var dim = _db.DimensionLevels
                            .Include(x => x.Level)
                            .Include(x => x.Dimension)
                            .ThenInclude(d => d.DimensionJoiners)
                            .Where(x => reportLevelIds
                                        .Concat(reportFilterLevelIds)
                            .Contains(x.LevelId) && x.Dimension.DeviceId == subquery.DeviceId)
                            .ToList();

                subquery.ReportSubqueryDimensions = dim
                                                    .GroupBy(y => new { y.Dimension.TableName, y.Dimension.DimensionJoiners })
                                                    .Select(g => new ReportSubqueryDimension
                                                    {
                                                        DimensionJoiners = g.Key.DimensionJoiners.ToList(),
                                                        DimensionTableName = g.Key.TableName,
                                                        LevelColumns = report.Levels.Select(x => new ReportLevelSubquery
                                                        {
                                                            LevelColumn = g.FirstOrDefault(y => y.LevelId == x.LevelId)?.ColumnName,
                                                            LevelName = g.FirstOrDefault(y => y.LevelId == x.LevelId)?.ColumnName,
                                                            LevelOrderByColumn = g.FirstOrDefault(y => y.LevelId == x.LevelId)?.OrderBy,
                                                            SortDirection = x.SortDirection.GetDisplayName()
                                                        }).Where(l => l.LevelColumn != null).ToList()
                                                    }).ToList();
                subquery.FilterContainers = report.ContainerOfFilters.Select(x => new ReportFilterContainerSubqueryModel
                {
                    LogicalOperator = x.LogicalOperator.GetDisplayName(),
                    Filters = x.ReportFilters.Select(f => new ReportFilterWithValue
                    {
                        FilterColumnName = dim.Where(b => b.LevelId == f.LevelId).Select(b => b.ColumnName).FirstOrDefault(),
                        FilterTableName = dim.Where(b => b.LevelId == f.LevelId).Select(b => b.Dimension.TableName).FirstOrDefault(),
                        FilterValues = f.Value?.ToList(),
                        LogicalOperator = f.LogicalOperator.GetDisplayName(),
                        Type = dim.Where(b => b.LevelId == f.LevelId).Select(b => b.Level.DataType).FirstOrDefault(),

                    }).ToList()
                }).ToList();
                if (i > 0)
                {
                    sql += " FULL OUTER JOIN " + getSubquery(subquery, i) + "ON 1 = 1 ";
                    foreach (var level in subquery.ReportSubqueryDimensions.Select(x => x.LevelColumns).SelectMany(c => c))
                    {
                        sql += $"AND {nvlLevel(i - 1, $"\"{level.LevelName}\"")} = S{i}.\"{level.LevelName}\" ";
                    }

                }
                else
                {
                    sql += getSubquery(subquery, i);
                }


            }
            var levelSelectSql = "SELECT " + String.Join(", ", joinedSubQueryModel[0].ReportSubqueryDimensions.Select(x => x.LevelColumns).SelectMany(c => c).Select(x => nvlLevel(joinedSubQueryModel.Count - 1, $"\"{x.LevelName}\"") + $" AS \"{x.LevelName}\""));
            var levelOrderBySql = " ORDER BY " + String.Join(", ", joinedSubQueryModel[0].ReportSubqueryDimensions.Select(x => x.LevelColumns).SelectMany(c => c).Select(x => nvlLevel(joinedSubQueryModel.Count - 1, $"\"{x.LevelName}\"") + $" {x.SortDirection} "));
            var measureSelectSql = String.Join(", ", report
                .Measures
                .Select(m => $" ROUND({getMeasureQuery(m.Operation)},2)" +
                $" AS \"{m.DisplayName}\""));
            var havingSelectSql = String.Join(" ",report.Measures
                .SelectMany(m => m.Havings
                    .Select(h => $"{h.LogicOpt.GetDisplayName()} {getMeasureQuery(m.Operation)} {_db.Operators.FirstOrDefault(o => o.Id == h.OperatorId).Name} {h.Value}")));
            sql = levelSelectSql + ", " +
                    measureSelectSql +
                " FROM " + sql + " WHERE 1 = 1 " +
                havingSelectSql + levelOrderBySql;
            return sql;
        }
        private string getSubquery(ReportSubqueryModel subqueryModel, int index)
        {
            var levelColumnList = subqueryModel.ReportSubqueryDimensions.Select(x => String.Join(",", x.LevelColumns.Select(y => x.DimensionTableName + "." + y.LevelColumn + $" AS \"{y.LevelName}\"")));
            var groupByColumnList = subqueryModel.ReportSubqueryDimensions.Select(x => String.Join(",", x.LevelColumns.Select(y => x.DimensionTableName + "." + y.LevelColumn)));
            var levelColumns = String.Join(",", levelColumnList);
            var groupByColumns = String.Join(",", groupByColumnList);
            var measureColumnList = subqueryModel.ReportSubqueryMeasures.Select(x => String.Join(",", $"{x.Aggregation}({subqueryModel.SubsetTableName}.{x.ColumnName}) {x.Aggregation}_{x.CounterId} "));
            var measureColumns = String.Join(",", measureColumnList);
            var selectQuery = "SELECT " + levelColumns + "," + measureColumns + " ";
            var groupByQuery = "GROUP BY " + groupByColumns;

            var fromQuery = $"FROM {subqueryModel.SubsetTableName} {subqueryModel.SubsetTableName} ";
            var joinQuery = "";
            var whereQuery = " WHERE 1 = 1 ";
            //joinQuery += subqueryModel.ReportSubqueryDimensions
            //             .SelectMany(dimension => $"JOIN {dimension.DimensionTableName} {dimension.DimensionTableName} ON 1 = 1 "
            //             + dimension.DimensionJoiners.Select(joiner => $"AND {dimension.DimensionTableName}.{joiner.PkName} = {subqueryModel.SubsetTableName}.{joiner.FkName} "));
            foreach (var dimension in subqueryModel.ReportSubqueryDimensions)
            {
                joinQuery += $"JOIN {dimension.DimensionTableName} {dimension.DimensionTableName} ON 1 = 1 ";
                foreach (var joiner in dimension.DimensionJoiners)
                {
                    joinQuery += $"AND {dimension.DimensionTableName}.{joiner.PkName} = {subqueryModel.SubsetTableName}.{joiner.FkName} ";
                }
            }
            foreach (var container in subqueryModel.FilterContainers)
            {
                whereQuery += $"{container.LogicalOperator} ( 1 = 1 ";
                foreach (var filter in container.Filters)
                {
                    if (filter.Type == "List")
                    {
                        whereQuery += $"{filter.LogicalOperator} {filter.FilterTableName}.{filter.FilterColumnName} in ";
                        whereQuery += $"({String.Join(",", filter.FilterValues.Select(x => "'" + x + "'"))}) ";
                    }
                    else if (filter.Type.ToLower() == "date")
                    {
                        whereQuery += $"{filter.LogicalOperator} {filter.FilterTableName}.{filter.FilterColumnName} between ";
                        whereQuery += $"TO_DATE('{filter.FilterValues?[0]}00', 'YYYYMMDDHH24') AND TO_DATE('{filter.FilterValues?[1]}23', 'YYYYMMDDHH24') ";
                    }
                    else if (filter.Type.ToLower() == "date_min")
                    {
                        whereQuery += $"{filter.LogicalOperator} {filter.FilterTableName}.{filter.FilterColumnName} between ";
                        whereQuery += $"TO_DATE('{filter.FilterValues?[0]}0000', 'YYYYMMDDHH24MI') AND TO_DATE('{filter.FilterValues?[1]}2359', 'YYYYMMDDHH24MI') ";
                    }
                    else if (filter.Type.ToLower() == "date_day")
                    {
                        whereQuery += $"{filter.LogicalOperator} {filter.FilterTableName}.{filter.FilterColumnName} between ";
                        whereQuery += $"TO_DATE('{filter.FilterValues?[0]}', 'YYYYMMDD') AND TO_DATE('{filter.FilterValues?[1]}', 'YYYYMMDD') ";
                    }

                }
                whereQuery += ") ";
            }


            return " (" + selectQuery + fromQuery + joinQuery + whereQuery + groupByQuery + ") S" + index + " ";
        }
        private string nvlLevel(int index, string levelName)
        {
            if (index <= 0) return $"S{index}.{levelName}";
            return $"NVL({nvlLevel(index - 1, levelName)}, S{index}.{levelName})";
        }
        private string getMeasureQuery(OperationBinding rootOperation)
        {
            var sql = "";
            foreach (OperationBinding operation in rootOperation.Childs)
            {
                switch(operation.Type)
                {
                    case (enOPerationTypes.counter):
                        {
                            sql += $" {operation.Aggregation.GetDisplayName()}_{operation.CounterId} ";
                            break;
                        }
                    case (enOPerationTypes.voidFunction):
                        {
                            sql += $"({operation.Childs.Select(x => getMeasureQuery(x))})";
                            break;
                        }
                    case (enOPerationTypes.kpi):
                        {
                            var item = _db.Kpis.Include(x => x.Operation).FirstOrDefault(x => x.Id == operation.KpiId);
                            sql += $" ({getMeasureQuery(item.Operation)}) ";
                            break;
                        }
                    case (enOPerationTypes.number):
                        {
                            sql += $" ({operation.Value}) ";
                            break;
                        }
                    case (enOPerationTypes.opt):
                        {
                            sql += $" {operation.Value} ";
                            break;
                        }
                    case (enOPerationTypes.function):
                        {
                            var fun = _db.Functions.FirstOrDefault(x => x.Id == operation.FunctionId);
                            if (fun.Name.ToLower() == "if")
                            {
                                sql += $"(CASE WHEN ({getMeasureQuery(operation.Childs[0])}) THEN ({getMeasureQuery(operation.Childs[1])}) ELSE ({getMeasureQuery(operation.Childs[2])}) END)";
                            }
                            else
                            {
                                sql += $" {fun.Name}({String.Join(", ", operation.Childs.Select(x => getMeasureQuery(x)))}) ";
                            }
                            break;
                        }
                    default:
                        {
                            sql += " ";
                            break;
                        }
                }
                
            }
            return sql;
        }
        private string getMeasureQuery(Operation rootOperation)
        {
            var sql = "";
            var operationChilds = _db.Operations.Include(x => x.Operator).Include(x => x.Function).Where(x => x.ParentId == rootOperation.Id);
            foreach (var operation in operationChilds.ToList() ?? [])
            {
                switch(operation.Type)
                {
                    case(enOPerationTypes.counter):
                        {
                            sql += $" {operation.Aggregation.GetDisplayName()}_{operation.CounterId} ";
                            break;
                        }
                    case(enOPerationTypes.voidFunction):
                    case (enOPerationTypes.kpi):
                        {
                            var childs = _db.Operations.Where(x => x.ParentId == operation.Id).ToList();
                            sql += $"({childs.Select(x => getMeasureQuery(x))})";
                            break;
                        }
                    case (enOPerationTypes.number):
                        {
                            sql += $" ({operation.Value}) ";
                            break;
                        }
                    case (enOPerationTypes.opt):
                        {
                            sql += $" {operation.Operator?.Name} ";
                            break;
                        }
                    case (enOPerationTypes.function):
                        {
                            var childs = _db.Operations.Where(x => x.ParentId == operation.Id).ToList();
                            if(operation.Function.Name.ToLower() == "if")
                            {
                                sql += $"(CASE WHEN ({getMeasureQuery(childs[0])}) THEN ({getMeasureQuery(childs[1])}) ELSE ({getMeasureQuery(childs[2])}) END)";
                            } else
                            {
                                sql += $" {operation.Function.Name}({String.Join(", ", childs.Select(x => getMeasureQuery(x)))}) ";
                            }
                            break;
                        }
                    default:
                        {
                            sql += " ";
                            break;
                        }
                }
            }
            return sql;
        }
    }
}
