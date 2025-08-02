using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Scenarios
{
    public class ScenarioContext
    {
        public long UserId { get; set; }
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public ScenarioContext( ScenarioType scenario)
        {
            CurrentScenario = scenario;
            Data = new Dictionary<string, object>();
        }
    }
}
