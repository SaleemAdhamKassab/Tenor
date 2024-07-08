using Azure;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using Tenor.Data;
using Tenor.Helper;

namespace Tenor.Services.DataServices
{
    public interface IDataProviderService
    {
        public List<string> fetchFilterOptionsByQuery(string query);
        IEnumerable<dynamic> fetchData(string query);
        int fetchCount(string query);
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
        public IEnumerable<dynamic> fetchData(string query)
        {
            using (var cmd = _dDb.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = query;
                if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object>;
                        for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                        {
                            row.Add(dataReader.GetName(fieldCount), (dataReader[fieldCount] is DBNull ? "" : dataReader[fieldCount]));
                        }
                        yield return row;
                    }
                }
            }
        }

        public int fetchCount(string query)
        {
            var result = _dDb.Database.SqlQueryRaw<int>($"{query}").ToList().FirstOrDefault();
            return result;
        }
    }
}
