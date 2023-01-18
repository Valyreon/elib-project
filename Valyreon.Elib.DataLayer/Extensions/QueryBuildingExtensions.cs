using System;
using System.Collections.Generic;

namespace Valyreon.Elib.DataLayer.Extensions
{
    internal static class QueryBuildingExtensions
    {
        public static string Apply(this string query, QueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Parameter 'query' can't be null or empty.");
            }

            if (parameters == null || (!parameters.HasSorting() && !parameters.HasPaging()))
            {
                return query;
            }

            if (parameters.HasSorting())
            {
                query += $" ORDER BY {parameters.SortBy.PropertyName}";
                query += $" {(parameters.SortBy.IsAscending ? "ASC" : "DESC")}";
            }

            if (parameters.HasPaging())
            {
                query += $" LIMIT {parameters.GetOffset()}, {parameters.PageSize}";
            }

            return query;
        }

        public static string Where(this string query, string condition)
        {
            return query + " WHERE "+ condition;
        }

        public static string And(this string query, string condition)
        {
            return query + " AND " + condition;
        }

        public static string And(this string query, IEnumerable<string> conditions)
        {
            return query + string.Join(" AND ", conditions);
        }

        public static string Or(this string query, string condition)
        {
            return query + " OR " + condition;
        }

        public static string Or(this string query, IEnumerable<string> conditions)
        {
            return query + string.Join(" OR ", conditions);
        }
    }
}
