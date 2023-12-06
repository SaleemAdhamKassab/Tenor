using System.Linq.Expressions;
using static Tenor.Helper.Constant;
using Tenor.Services.SubsetsService.ViewModels;

namespace Tenor.Helper
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(toLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(toLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> toLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propertyAsObject = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(propertyAsObject, parameter);
        }
    }
}
