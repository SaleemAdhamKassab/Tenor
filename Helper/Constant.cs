namespace Tenor.Helper
{
    public class Constant
    {
        public enum enOPerationTypes { counter, function, kpi, opt, voidFunction, number }
        public enum enAggregation { na,min, max, sum, avg, count }
        public enum fieldTypes { Text, MultiSelectList, List }
        public enum enSortDirection { asc, desc }
        public enum enAccessType { all , allOnlyMe , viewOnlyMe, denied }
    }
}
