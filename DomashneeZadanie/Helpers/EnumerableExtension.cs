using System;
using System.Collections.Generic;
using System.Linq;

namespace Domashneezadanie.Helpers
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// Возвращает подмножество элементов из последовательности, соответствующее указанной "пачке".
        /// </summary>
        /// <typeparam name="T">Тип элементов последовательности.</typeparam>
        /// <param name="source">Исходная последовательность.</param>
        /// <param name="batchSize">Размер пачки (количество элементов в одной пачке).</param>
        /// <param name="batchNumber">Номер пачки (нумерация с 0).</param>
        /// <returns>Подмножество элементов из последовательности.</returns>
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source, int batchSize, int batchNumber)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер пачки должен быть больше 0.");
            if (batchNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(batchNumber), "Номер пачки не может быть отрицательным.");

            return source.Skip(batchSize * batchNumber).Take(batchSize);
        }
    }
}