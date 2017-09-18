﻿using System;
using System.Linq;
using Newtonsoft.Json;
using Octopus.Shared.Model;
using Octopus.Shared.Security.Masking;

namespace Octopus.Shared.Diagnostics
{
    public class LogContext
    {
        readonly string[] sensitiveValues;
        readonly string correlationId;
        readonly LogContext parent;
        readonly object sensitiveDataMaskLock = new object();
        SensitiveDataMask sensitiveDataMask;
        AhoCorasick trie;

        [JsonConstructor]
        public LogContext(string correlationId = null, string[] sensitiveValues = null, LogContext parent = null)
        {
            this.correlationId = correlationId ?? GenerateId();
            this.sensitiveValues = sensitiveValues ?? new string[0];
            this.parent = parent ?? this;
        }

        public string CorrelationId => correlationId;

        [Encrypted]
        public string[] SensitiveValues => sensitiveValues;

        public void SafeSanitize(string raw, Action<string> action)
        {
            try
            {
                // JIT creation of sensitiveDataMask
                if (sensitiveDataMask == null && sensitiveValues.Length > 0)
                    lock (sensitiveDataMaskLock)
                    {
                        if (sensitiveDataMask == null && sensitiveValues.Length > 0)
                        {
                            sensitiveDataMask = new SensitiveDataMask();
                            trie = new AhoCorasick();
                            foreach (var instance in sensitiveValues)
                            {
                                if (string.IsNullOrWhiteSpace(instance) || instance.Length < 4)
                                    continue;

                                var normalized = instance.Replace("\r\n", "").Replace("\n", "");

                                trie.Add(normalized);
                            }

                            trie.Build();
                        }
                    }

                // Chain action with parents SafeSanitize
                Action<string> actionWithParent = s =>
                {
                    if (parent == this)
                        action(s);
                    else
                        parent.SafeSanitize(s, action);
                };

                if (sensitiveDataMask != null)
                    sensitiveDataMask.ApplyTo(trie, raw, actionWithParent);
                else
                    actionWithParent(raw);
            }
            catch
            {
                action(raw);
            }
        }

        public LogContext CreateSibling(string[] sensitiveValues = null) => Parent().CreateChild(sensitiveValues);

        public LogContext Parent() => parent;

        public LogContext CreateChild(string[] sensitiveValues = null) => new LogContext((correlationId + '/' + GenerateId()), this.sensitiveValues.Union(sensitiveValues ?? new string[0]).ToArray(), this);

        static string GenerateId() => Guid.NewGuid().ToString("N");

        public void Flush()
        {
            sensitiveDataMask?.Flush(trie);
        }
    }
}