using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NetCore.Extensions.Core.Data;

namespace NetCore.Extensions.Core.FindOptions
{
    public class FindOptions<TRecord>
    {
        public FindOptions()
        {
            Sorts = new Dictionary<LambdaExpression, SortDirection>();
        }

        public IDictionary<LambdaExpression, SortDirection> Sorts { get; private set; }

        public int? Skip { get; set; }

        public int? Limit { get; set; }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, string>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, bool>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, int>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, long>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, float>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, decimal>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, DateTime>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, Guid>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortAscending(Expression<Func<TRecord, Guid?>> field)
        {
            Sorts.Add(field, SortDirection.Ascending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, string>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, bool>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, int>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, long>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, float>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, decimal>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, DateTime>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, Guid>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }

        public FindOptions<TRecord> SortDescending(Expression<Func<TRecord, Guid?>> field)
        {
            Sorts.Add(field, SortDirection.Descending);
            return this;
        }
    }
}
