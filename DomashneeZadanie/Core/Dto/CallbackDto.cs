using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Dto
{
    public class CallbackDto
    {
        public string Action { get; set; }
 
        public CallbackDto(string action)
        {
            Action = action;
        }

        public static CallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new CallbackDto(string.Empty);
            }

            string[] parts = input.Split('|');

            string action = string.Empty;
            if (parts.Length > 0)
            {
                action = parts[0];
            }

            return new CallbackDto(action);
        }

        public override string ToString()
        {
            if (Action == null)
            {
                return string.Empty;
            }
            return Action;
        }
    }

}
