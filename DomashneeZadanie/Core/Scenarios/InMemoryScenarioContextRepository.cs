﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _contexts = new();
        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            _contexts.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }
        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            context.UserId = userId;
            _contexts[userId] = context;
            return Task.CompletedTask;
        }
        public Task ResetContext(long userId, CancellationToken ct)
        {
            _contexts.TryRemove(userId, out _);
            return Task.CompletedTask;
        }
    }
}
