using static Tenor.Services.KpisService.ViewModels.KpiModels;
using Tenor.Dtos;
using Tenor.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;

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
            var opt = _db.Operations.Include(x => x.Function).Include(x => x.Counter)
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

                //if (opt.Childs.Count != 0)
                //{
                //    foreach (var c in opt.Childs)
                //    {

                //        GetQeuryExpress(c, null);

                //    }
                //}
            }
            if (opt.Type == "opt")
            {

                qe.LeftSide = ""; qe.Inside = opt.OperatorName; qe.RightSide = "";
                query = query.Insert(query.Length - 1, qe.LeftSide + qe.Inside + qe.RightSide);


                //if (opt.Childs.Count != 0)
                //{
                //    foreach (var c in opt.Childs)
                //    {

                //        GetQeuryExpress(c, null);



                //    }
                //}
            }
            if (opt.Type == "kpi")
            {
                string kpiNewFormat = GetKpiFomat((int)opt.KpiId);
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
                qe.Inside = opt.Aggregation == "na" ? opt.CounterName : opt.Aggregation + "(" + opt.CounterName + ")";
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
                query = query.Insert(query.Length - 1, Chang);


                if (opt.Childs.Count != 0)
                {

                    foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                    {
                        //GetQeuryExpress(c.value, funcTag + c.i);
                        string funcParam = funcTag + c.i;
                        funcParam =GetQeuryExpress(c.value, null);
                        query = query.Replace(funcParam,funcParam);
                    }
                }
            }

            return query;

        }




    }
}
