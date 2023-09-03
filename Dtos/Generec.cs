namespace Tenor.Dtos
{
    public class DataWithSize
    {
        public List<object> Data { get; set; }
        public int DataSize { get; set; }
    }
    public class ResultWithMessage
    {
        public ResultWithMessage(object data, string message)
        {
            Data = data;
            Message = message;
        }
        public Object? Data { get; set; }
        public string? Message { get; set; }
    }
}
