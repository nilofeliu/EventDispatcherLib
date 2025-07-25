using System;
using System.Collections.Generic;

namespace EventDispatcher.Model
{
    public class HandlerResult
    {
        public bool Success { get; init; }
        public string Message { get; init; }
        public Exception Exception { get; init; }
        private Dictionary<string, object> _metadata { get; } = new();
        public IReadOnlyDictionary<string, object> Metadata => _metadata;

        public static HandlerResult Ok(string message = null, Dictionary<string, object> metadata = null)
        {
            var result = new HandlerResult
            {
                Success = true,
                Message = message
            };

            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    result._metadata.Add(item.Key, item.Value);
                }
            }

            return result;
        }

        public static HandlerResult Fail(string message, Exception ex = null,
                                      Dictionary<string, object> metadata = null)
        {
            var result = new HandlerResult
            {
                Success = false,
                Message = message,
                Exception = ex
            };

            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    result._metadata.Add(item.Key, item.Value);
                }
            }

            return result;
        }
    }
}