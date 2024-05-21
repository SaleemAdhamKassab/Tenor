using Azure;
using Microsoft.EntityFrameworkCore;
using Tenor.Data;

namespace Tenor.Services.DataServices
{
    public interface IDataProviderService
    {
        public List<string> fetchFilterOptionsByQuery(string query);
    }
    public class DataProviderService : IDataProviderService
    {
        private readonly DataContext _dDb;
        public DataProviderService(DataContext dDb)
        {
            _dDb = dDb;
        }

        public List<string> fetchFilterOptionsByQuery(string query)
        {
            var result = _dDb.Database.SqlQueryRaw<string>($"{query}").ToList();
            return result ?? new List<string>();
        }
    }
}
