using System.Collections.Generic;

namespace devoft.ClientModel
{
    public static class FluentCollection
    {
        public static IList<T> FluentAdd<T>(this IList<T> col, T item)
        {
            col.Add(item);
            return col;
        }

        public static IList<T> FluentInsert<T>(this IList<T> col, int index, T item)
        {
            col.Insert(index, item);
            return col;
        }
    }
}
