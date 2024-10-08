﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Helper.Constant;
using static Tenor.Services.SharedService.ViewModels.SharedModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
namespace Tenor.Services.ReportService.ViewModels
{
	public class ReportModels
	{
		public class CreateReport
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int DeviceId { get; set; }
			public List<ReportMeasureDto> Measures { get; set; }
			public List<ReportLevelDto> Levels { get; set; }
			public List<ContainerOfFilter> ContainerOfFilters { get; set; }
			public List<ExtraFieldValue>? ReportFields { get; set; }
			public bool IsPublic { get; set; }
			public int? ChildId { get; set; }
			public string? CreatedBy { get; set; }
			public DateTime? CreatedDate { get; set; }

		}
		public class ReportMeasureDto
		{
			public int Id { get; set; }
			public string DisplayName { get; set; }
			public OperationBinding Operation { get; set; }
			public List<Having> Havings { get; set; }
		}
		public class Having
		{
			public int Id { get; set; }
			public int? OperatorId { get; set; }
			public enLogicalOperator LogicOpt { get; set; }
			public double? Value { get; set; }
		}
		public class HavingViewModel
		{

			public int Id { get; set; }
			public int? OperatorId { get; set; }
			public string? OperatorName { get; set; }
			public enLogicalOperator LogicOpt { get; set; }
			public string? LogicOptName { get; set; }
			public int? Value { get; set; }
		}
		public class ReportLevelDto
		{
			public int Id { get; set; }
			public int DisplayOrder { get; set; }
			public enSortDirection SortDirection { get; set; }
			public int LevelId { get; set; }
		}
		public class ReportLevelViewModel
		{
			public int Id { get; set; }
			public int DisplayOrder { get; set; }
			public enSortDirection SortDirection { get; set; }
			public string SortDirectionName { get; set; }
			public int LevelId { get; set; }
			public string LevelName { get; set; }
			public bool IsLevel { get; set; }
			public bool IsFilter { get; set; }
		}
		public class ReportFilterDto
		{
			public int Id { get; set; }
			public enLogicalOperator LogicalOperator { get; set; }
			public string? LogicalOperatorName { get; set; }
			public string[]? Value { get; set; }
			public int LevelId { get; set; }
			public string? LevelName { get; set; }
			public string? Type { get; set; }
			public bool IsMandatory { get; set; }
			public bool IsVariable { get; set; }

		}
		public class ContainerOfFilter
		{
			public int Id { get; set; }
			public enLogicalOperator LogicalOperator { get; set; }
			public string? LogicalOperatorName { get; set; }
			public List<ReportFilterDto> ReportFilters { get; set; }
		}
		public class MeasureViewModel
		{
			public int Id { get; set; }
			public string DisplayName { get; set; }
			public OperationDto Operation { get; set; }
			public List<HavingViewModel> Havings { get; set; }

		}
		public class ReportViewModel
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int DeviceId { get; set; }
			public string DeviceName { get; set; }
			public bool IsPublic { get; set; }
			public string? CreatedBy { get; set; }
			public DateTime? CreatedDate { get; set; }
			public int? ChildId { get; set; }
			public bool CanEdit { get; set; }
			public List<MeasureViewModel> Measures { get; set; }
			public List<ReportLevelViewModel> Levels { get; set; }
			public List<ReportFieldValueViewModel>? ReportFields { get; set; }
			public List<ContainerOfFilter> ContainerOfFilters { get; set; }

		}
		public class ReportFieldValueViewModel
		{
			public int Id { get; set; }
			public int FieldId { get; set; }
			public string Type { get; set; }
			public string FieldName { get; set; }
			public object Value { get; set; }
		}
		public class ReportDto
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int DeviceId { get; set; }
			public string DeviceName { get; set; }
			public bool IsPublic { get; set; }
			public string? CreatedBy { get; set; }
			public DateTime? CreatedDate { get; set; }
			public bool CanEdit { get; set; }
		}
		public class ReportListFilter : GeneralFilterModel
		{
			public int DeviceId { get; set; }
			public IDictionary<string, object>? ExtraFields { get; set; }

		}
		public class ReportTreeFilter
		{
			public string? SearchQuery { get; set; }
			public int? deviceId { get; set; }
			public string? userName { get; set; }
			public IDictionary<string, object>? ExtraFields { get; set; }

		}
		public class TreeReportViewModel
		{
			public int? Id { get; set; }
			public string? Name { get; set; }
			public string? Type { get; set; }
			public bool HasChild { get; set; }
			public bool CanEdit { get; set; }
		}

		public class DimensionLevelsViewModel
		{
			public string DimensionName { get; set; }
			public List<DimensionLevel> DimensionLevels { get; set; }
		}
		public class ReportPreviewColumnModel
		{
			public string? Name { get; set; }
			public string? Type { get; set; }
		}
		public class ReportRehearsalModel
		{
			public string? Name { get; set; }
			public List<ReportPreviewColumnModel> Columns { get; set; } = new List<ReportPreviewColumnModel>();
            public List<ContainerOfFilter> ContainerOfFilters { get; set; } = new List<ContainerOfFilter>();
			public bool canEdit { get; set; }
        }
	}
}
