using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetCore.Extensions.Core.Data
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

        //public void TranslateSortOptions(IEnumerable<SortDescriptor> sorts)
        //{
        //    foreach (var sortDescriptor in sorts)
        //    {
        //        var parameter = Expression.Parameter(typeof(TRecord));
        //        var memberExpression = Expression.Property(parameter, typeof(TRecord).GetProperty(sortDescriptor.Member));

        //        if (memberExpression.Type.IsEnum)
        //        {
        //            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TRecord), memberExpression.Type);
        //            Sorts.Add(Expression.Lambda(delegateType, memberExpression, parameter), sortDescriptor.SortDirection);
        //        }
        //        else
        //        {
        //            Type type = memberExpression.Type;
        //            bool isNullable = type.IsNullable();

        //            if (isNullable)
        //            {
        //                type = Nullable.GetUnderlyingType(type);
        //            }

        //            switch (Type.GetTypeCode(type))
        //            {
        //                case TypeCode.String:
        //                    Sorts.Add(Expression.Lambda<Func<TRecord, string>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    break;

        //                case TypeCode.Boolean:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, bool?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, bool>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                case TypeCode.Byte:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, byte?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, byte>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                case TypeCode.Int16:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, short?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, short>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                case TypeCode.Int32:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, int?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, int>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                case TypeCode.Int64:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, long?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, long>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                case TypeCode.Single:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, float?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, float>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                case TypeCode.Decimal:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, decimal?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, decimal>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                case TypeCode.Double:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, double?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, double>>(memberExpression, parameter), sortDescriptor.SortDirection);

        //                    }
        //                    break;

        //                case TypeCode.DateTime:
        //                    if (isNullable)
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, DateTime?>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, DateTime>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    break;

        //                default:
        //                    if (memberExpression.Type == typeof(Guid))
        //                    {
        //                        Sorts.Add(Expression.Lambda<Func<TRecord, Guid>>(memberExpression, parameter), sortDescriptor.SortDirection);
        //                    }
        //                    else
        //                    {
        //                        var body = Expression.Convert(memberExpression, typeof(object));
        //                        var defaultLambdaExpression = Expression.Lambda<Func<TRecord, object>>(memberExpression, parameter);
        //                        Sorts.Add(defaultLambdaExpression, sortDescriptor.SortDirection);
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //}
    }
}
