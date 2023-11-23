namespace Tenor.Dtos
{
    public class DataWithSize<T> where T : class
    {
        public DataWithSize(int dataSize, List<T> data)
        {
            DataSize = dataSize;
            Data = data;
        }

        public int DataSize { get; set; }
        public List<T> Data { get; set; }
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
