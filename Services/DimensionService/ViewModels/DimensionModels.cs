namespace Tenor.Services.DimensionService.ViewModels
{
    public class DimensionModels
    {
        public class DimensionViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool HasChild { get; set; }
            public List<DimLevelViewModel> Levels { get; set; }
        }

        public class DimLevelViewModel
        {
            public int DimLevelId { get; set; }
            public string DimLevelColumnName { get;set; }
            public int LevelId { get; set; }
            public string LevelName { get; set; }
            public string Order { get; set; }

        }
    }
}
