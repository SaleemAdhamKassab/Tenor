using static Tenor.Services.KpisService.ViewModels.KpiModels;
using Tenor.Dtos;
using Tenor.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using Tenor.Services.KpisService;

namespace Tenor.Helper
{
    public   class ConvertKpiForm
    {
        private  readonly TenorDbContext _db;
        private   readonly IMapper _mapper;
        private   string query = "";
        public   ConvertKpiForm( TenorDbContext tenorDbContext, IMapper mapper)
        {
            _db = tenorDbContext;
            _mapper = mapper;
            
        }

        public  string GetKpiFomat(int kpiid)
        {
            var kpi = _db.Kpis.Include(x => x.Device).Include(x => x.Operation).Include(x => x.KpiFieldValues)
                .ThenInclude(x => x.KpiField).ThenInclude(x => x.ExtraField)
                .FirstOrDefault(x => x.Id == kpiid);

            if (kpi == null)
            {
                return null;

            }
            GetSelfRelation(kpi.OperationId);
            var kpiMap = _mapper.Map<KpiViewModel>(kpi);
            string queryBuilder = GetQeuryExpress(kpiMap.Operations, null);

            return queryBuilder;
        }

        private List<OperationDto> GetSelfRelation(int optid)
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

        public string GetQeuryExpress(OperationDto opt, string? tag)
        {

            string pointerTag = "tag";
            string funcTag = "func";
            QueryExpress qe = new QueryExpress();
            KpisService kpiFormat = new KpisService(_db, _mapper);
            IDictionary<string, string> funcDic = new Dictionary<string, string>()
            {
                {"func0","Case When" },{"func1","Then" },{"func2","else" }
            };

            if (opt.Type == "voidFunction")
            {
                if (!string.IsNullOrEmpty(tag) || query.Contains(pointerTag))
                {
                    qe.LeftSide = "("; qe.Inside = pointerTag; qe.RightSide = "";
                    string chageStr = qe.LeftSide + qe.Inside + qe.RightSide;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        query = query.Replace(tag, chageStr);

                    }
                    else
                    {
                        query = query.Replace(pointerTag, chageStr);

                    }

                    if (opt.Childs.Count != 0)
                    {
                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {
                            GetQeuryExpress(c.value, null);

                        }
                        query = query + ")";
                    }
                }
                else
                {

                    qe.LeftSide = "("; qe.Inside = pointerTag; qe.RightSide = ")";
                    query += qe.LeftSide + qe.Inside + qe.RightSide;
                    if (opt.Childs.Count != 0)
                    {
                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {

                            GetQeuryExpress(c.value, null);

                        }
                    }
                }

            }
            if (opt.Type == "number")
            {

                qe.LeftSide = ""; qe.Inside = opt.Value; qe.RightSide = "";
                string repStr = qe.LeftSide + qe.Inside + qe.RightSide;
                if (query.Contains(pointerTag))
                {
                    query = query.Replace(pointerTag, repStr);
                }
                else
                {
                    query = query.Insert(query.Length - 1, repStr);

                }

               
            }
            if (opt.Type == "opt")
            {

                qe.LeftSide = ""; qe.Inside = opt.OperatorName; qe.RightSide = "";
                string changeStr = qe.LeftSide + qe.Inside + qe.RightSide;
                if (query.Contains(pointerTag))
                {
                    query = query.Replace(pointerTag, changeStr);
                }
                else
                {
                    query = query.Insert(query.Length - 1, changeStr);

                }

            }
            if (opt.Type == "kpi")
            {
                string kpiNewFormat = kpiFormat.GetKpiForm((int)opt.KpiId).Result.Data.ToString();
                qe.LeftSide = "(";
                qe.Inside = opt.Aggregation == "na" ? kpiNewFormat : opt.Aggregation + "(" + kpiNewFormat + ")";
                qe.RightSide = ")";
                string kpiState = qe.LeftSide + qe.Inside + qe.RightSide;
                if (!query.Contains(pointerTag))
                {
                    query = query.Insert(query.Length - 1, kpiState);

                }
                else
                {
                    query = query.Replace(pointerTag, kpiState);

                }
                //if (opt.Childs.Count != 0)
                //{
                //    foreach (var c in opt.Childs)
                //    {

                //        GetQeuryExpress(c, null);

                //    }
                //}
            }
            if (opt.Type == "counter")
            {

                qe.LeftSide = "";
                qe.Inside = opt.Aggregation == "na" ? opt.TableName + "." + opt.ColumnName : opt.Aggregation + "(" + opt.TableName + "." + opt.ColumnName + ")";
                qe.RightSide = "";
                string chageStr = qe.LeftSide + qe.Inside + qe.RightSide;
                if (!query.Contains(pointerTag))
                {
                    query = query.Insert(query.Length - 1, chageStr);

                }
                else
                {
                    query = query.Replace(pointerTag, chageStr);

                }
                if (opt.Childs.Count != 0)
                {
                    foreach (var c in opt.Childs)
                    {

                        GetQeuryExpress(c, null);
                    }

                }


            }
            if (opt.Type == "function")
            {

                var func = _db.Functions.FirstOrDefault(f => f.Id == opt.FunctionId);
                if(func.Name.ToLower()=="if")
                {
                    qe.Inside = "Case when func0 then func1 else func2";
                    qe.LeftSide = opt.FunctionName + "(";
                    qe.RightSide = ")";
                    string Chang = qe.LeftSide + qe.Inside + qe.RightSide;
                    if (query.Contains(pointerTag))
                    {
                        query = query.Replace(pointerTag, Chang);

                    }
                    else
                    {
                        query = query.Insert(query.Length > 0 ? query.Length - 1 : 0, Chang);

                    }

                    if (opt.Childs.Count != 0)
                    {

                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {
                            kpiFormat = new KpisService(_db, _mapper);
                            string funcParam = funcTag + c.i;
                            string repFuncParam = kpiFormat.GetQeuryExpress(c.value, null);
                            query = query.Replace(funcParam, repFuncParam);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i <= func.ArgumentsCount - 1; i++)
                    {
                        if (opt.Aggregation == "na")
                        {
                            qe.Inside += i < func.ArgumentsCount - 1 ? funcTag + i + "," : funcTag + i;

                        }
                        else
                        {
                            qe.Inside += i < func.ArgumentsCount - 1 ? opt.Aggregation + "(" + funcTag + i + ")" + "," : opt.Aggregation + "(" + funcTag + i + ")";

                        }



                    }
                    qe.LeftSide = opt.FunctionName + "(";
                    qe.RightSide = ")";
                    string Chang = qe.LeftSide + qe.Inside + qe.RightSide;
                    if (query.Contains(pointerTag))
                    {
                        query = query.Replace(pointerTag, Chang);

                    }
                    else
                    {
                        query = query.Insert(query.Length > 0 ? query.Length - 1 : 0, Chang);

                    }

                    if (opt.Childs.Count != 0)
                    {

                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {
                            kpiFormat = new KpisService(_db, _mapper);
                            string funcParam = funcTag + c.i;
                            string repFuncParam = kpiFormat.GetQeuryExpress(c.value, null);
                            query = query.Replace(funcParam, repFuncParam);
                        }
                    }
                }
              
            }

            return query;

        }




    }
}
