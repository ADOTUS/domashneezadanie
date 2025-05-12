using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Services
{
    public class NamePrefixFind
    {
        private readonly string _prefix;

        public NamePrefixFind(string prefix)
        {
            _prefix = prefix.ToLowerInvariant();
        }

        public bool IsMatch(ToDoItem item)
        {
            if (item.Name == null)
                return false;

            return item.Name.ToLowerInvariant().StartsWith(_prefix);
        }
    }
}
