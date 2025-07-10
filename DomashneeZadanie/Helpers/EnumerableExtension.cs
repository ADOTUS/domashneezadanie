using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Helpers
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source, int batchSize, int batchNumber)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер пачки не может быть 0.");
            if (batchNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(batchNumber), "Номер пачки не может быть отрицательным.");

            return source.Skip(batchSize * batchNumber).Take(batchSize);
        }
    }
}
