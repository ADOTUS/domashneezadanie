using System;
using System.Collections.Generic;
using System.Linq;

namespace Domashneezadanie.Helpers
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// ���������� ������������ ��������� �� ������������������, ��������������� ��������� "�����".
        /// </summary>
        /// <typeparam name="T">��� ��������� ������������������.</typeparam>
        /// <param name="source">�������� ������������������.</param>
        /// <param name="batchSize">������ ����� (���������� ��������� � ����� �����).</param>
        /// <param name="batchNumber">����� ����� (��������� � 0).</param>
        /// <returns>������������ ��������� �� ������������������.</returns>
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source, int batchSize, int batchNumber)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "������ ����� ������ ���� ������ 0.");
            if (batchNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(batchNumber), "����� ����� �� ����� ���� �������������.");

            return source.Skip(batchSize * batchNumber).Take(batchSize);
        }
    }
}