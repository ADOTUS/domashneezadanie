using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Type { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTime ScheduledAt { get; set; }
        public bool IsNotified { get; set; }
        public DateTime? NotifiedAt { get; set; }
    }
}
