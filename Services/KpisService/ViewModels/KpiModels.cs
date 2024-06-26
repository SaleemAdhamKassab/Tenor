﻿using static Tenor.Helper.Constant;
using System.ComponentModel.DataAnnotations;
using Tenor.Dtos;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using static Tenor.Services.SharedService.ViewModels.SharedModels;

namespace Tenor.Services.KpisService.ViewModels
{
    public class KpiModels
    {
        public class CreateKpi
        {
            public int Id { get; set; }
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
            [Required]
            public string Name { get; set; }
            public int? DeviceId { get; set; }
            public string ? CreatedBy { get; set; }
            public DateTime CreationDate { get; set; }=DateTime.Now;
            public bool IsPublic { get; set; }
            public string? ModifyBy { get; set; }
            public DateTime? ModifyDate { get; set; } = null;
            public string? DeletedBy { get; set; }
            public DateTime? DeletedDate { get; set; } = null;
            public List<ExtraFieldValue>? KpiFields { get; set; }
            public OperationBinding Operation { get; set; }

        }
        public class OperationDto
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public string? Value { get; set; }
            public enOPerationTypes Type { get; set; }
            public enAggregation Aggregation { get; set; }
            public int? CounterId { get; set; }
            public string? CounterName { get; set; }
            public int? KpiId { get; set; }
            public string? KpiName { get; set; }
            public int? FunctionId { get; set; }
            public string? FunctionName { get; set; }
            public int? OperatorId { get; set; }
            public string? OperatorName { get; set; }
            public int? ParentId { get; set; } //Self Join
            public int ? SubsetId { get; set; }
            public string ? SubsetName { get; set; }
            public string? TableName { get; set; }
            public string ? ColumnName { get; set; }

            public List<OperationDto>? Childs { get; set; }

        }
        public class KpiExtraField
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public dynamic Content { get; set; }
            public string? Url { get; set; }
            public bool IsMandatory { get; set; }
            public int? DeviceId { get; set; }
            public string ? DeviceName { get; set; }

        }
        public class KpiFieldValueViewModel
        {
            public int Id { get; set; }
            public int FieldId { get; set; }
            public string Type { get; set; }
            public string FieldName { get; set; }
            public object Value { get; set; }
        }
        public class KpiViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? DeviceId { get; set; }
            public int OperationId { get; set; }

            public string? DeviceName { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreationDate { get; set; }
            public bool IsPublic { get; set; }
            public string? ModifyBy { get; set; }
            public DateTime? ModifyDate { get; set; }
            public string? DeletedBy { get; set; }
            public DateTime? DeletedDate { get; set; }
            public List<KpiFieldValueViewModel> ? ExtraFields { get; set; }
            public OperationDto Operations { get; set; }

        }
        public class  KpiFilterModel: GeneralFilterModel
        {
            public int ? DeviceId { get; set; }
            public IDictionary<string, object>? ExtraFields { get; set; }

        }
        public class Filter
        {
            public string key { get; set; }
            public object values { get; set; }
        }      
        public class KpiListViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? DeviceId { get; set; }
            public string? DeviceName { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreationDate { get; set; }
            public bool IsPublic { get; set; }
            public string? ModifyBy { get; set; }
            public DateTime? ModifyDate { get; set; }
            public string? DeletedBy { get; set; }
            public DateTime? DeletedDate { get; set; }
            public List<KpiFieldValueViewModel> ? ExtraFields { get; set; }

        }
        public class QueryExpress
        {
            public string LeftSide { get; set; }
            public string Inside { get; set; }
            public string RightSide { get; set; }
        }
        public class QueryWithJoinExpress
        {
            public string basicQuery { get; set; }
            public List<string> tables { get; set; }
        }
    }
}
