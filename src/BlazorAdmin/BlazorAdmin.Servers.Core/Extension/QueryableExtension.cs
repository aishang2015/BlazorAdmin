using BlazorAdmin.Core.Extension;
using System.Linq.Expressions;

namespace BlazorAdmin.Core.Extension
{
    public static class QueryableExtension
    {
        public static IQueryable<T> And<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate) where T : class
            => queryable.Where(predicate);

        public static IQueryable<T> AndIf<T>(this IQueryable<T> queryable, bool condition, Expression<Func<T, bool>> predicate) where T : class
            => condition ? queryable.Where(predicate) : queryable;

        public static IQueryable<T> AndIfExist<T>(this IQueryable<T> queryable, string? value,
            Expression<Func<T, bool>> predicate) where T : class
            => string.IsNullOrEmpty(value) ? queryable : queryable.Where(predicate);

        public static IQueryable<T> AndIfExist<T, TData>(this IQueryable<T> queryable, TData value,
            Expression<Func<T, bool>> predicate) where T : class
            => value is null ? queryable : queryable.Where(predicate);

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, params (Expression<Func<T, object>>, bool)[] keySelectors) where T : class
        {
            foreach (var keySelector in keySelectors)
            {
                if (queryable is IOrderedQueryable<T>)
                {
                    queryable = keySelector.Item2 ?
                           (queryable as IOrderedQueryable<T>).ThenBy(keySelector.Item1) :
                           (queryable as IOrderedQueryable<T>).ThenByDescending(keySelector.Item1);
                }
                else
                {
                    queryable = keySelector.Item2 ?
                        queryable.OrderBy(keySelector.Item1) :
                        queryable.OrderByDescending(keySelector.Item1);

                }
            }
            return queryable;
        }

        public static IQueryable<T> QueryPage<T>(this IQueryable<T> queryable, int page, int size)
        {
            return queryable.Skip((page - 1) * size).Take(size);
        }

        public static IList<T> AndIf<T>(this IList<T> queryable, bool condition, Func<T, bool> predicate) where T : class
            => condition ? queryable.Where(predicate).ToList() : queryable;
    }
}