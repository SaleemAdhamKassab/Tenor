using static Tenor.Helper.Constant;

namespace Tenor.Dtos
{
    public class OperstionViewModel
    {
        public int KpiId { get; set; }
        public int CounterId { get; set; }
        public int FunctionId { get; set; }
        public int OperatorId { get; set; }
        public string Value { get; set; }
        public enOPerationTypes Type { get; set; }
    }
}