// EventDispatcher/Serialization/EventTypeRegistry.cs
using EventDispatcher.Contracts;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace EventDispatcher.Serialization
{
    /// <summary>
    /// Registry for mapping event type names to actual .NET types.
    /// Supports both assembly-qualified names and short names for flexibility.
    /// </summary>
    public class EventTypeRegistry
    {
        private readonly ConcurrentDictionary<string, Type> _typeCache = new();
        private readonly ConcurrentDictionary<Type, string> _nameCache = new();

        /// <summary>
        /// Registers an event type with an optional custom name.
        /// If no custom name is provided, uses the type's full name.
        /// </summary>
        public void RegisterEventType<T>(string customName = null) where T : IEvent
        {
            RegisterEventType(typeof(T), customName);
        }

        /// <summary>
        /// Registers an event type with an optional custom name.
        /// </summary>
        public void RegisterEventType(Type eventType, string customName = null)
        {
            if (!typeof(IEvent).IsAssignableFrom(eventType))
                throw new ArgumentException($"Type {eventType.Name} must implement IEvent", nameof(eventType));

            var typeName = customName ?? eventType.FullName;

            _typeCache.TryAdd(typeName, eventType);
            _nameCache.TryAdd(eventType, typeName);
        }

        /// <summary>
        /// Auto-discovers and registers all IEvent implementations in the specified assemblies.
        /// Uses full type names as registration keys.
        /// </summary>
        public void AutoRegisterFromAssemblies(params Assembly[] assemblies)
        {
            if (assemblies?.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };

            foreach (var assembly in assemblies)
            {
                var eventTypes = assembly.GetTypes()
                    .Where(t => typeof(IEvent).IsAssignableFrom(t) &&
                               !t.IsInterface &&
                               !t.IsAbstract);

                foreach (var eventType in eventTypes)
                {
                    RegisterEventType(eventType);
                }
            }
        }

        /// <summary>
        /// Resolves a type name to its corresponding .NET Type.
        /// Tries registered names first, then falls back to Type.GetType().
        /// </summary>
        public Type ResolveType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));

            // Try cache first (registered types)
            if (_typeCache.TryGetValue(typeName, out var cachedType))
                return cachedType;

            // Fallback to reflection-based resolution
            var resolvedType = Type.GetType(typeName, throwOnError: false);

            if (resolvedType != null && typeof(IEvent).IsAssignableFrom(resolvedType))
            {
                // Cache for future lookups
                _typeCache.TryAdd(typeName, resolvedType);
                _nameCache.TryAdd(resolvedType, typeName);
                return resolvedType;
            }

            throw new InvalidOperationException($"Unable to resolve event type: {typeName}");
        }

        /// <summary>
        /// Gets the registered name for a given type.
        /// Falls back to FullName if not explicitly registered.
        /// </summary>
        public string GetTypeName(Type eventType)
        {
            if (_nameCache.TryGetValue(eventType, out var cachedName))
                return cachedName;

            // Fallback to full name
            var typeName = eventType.FullName;
            _nameCache.TryAdd(eventType, typeName);
            return typeName;
        }

        /// <summary>
        /// Clears all registered types.
        /// </summary>
        public void Clear()
        {
            _typeCache.Clear();
            _nameCache.Clear();
        }

        /// <summary>
        /// Gets count of registered types.
        /// </summary>
        public int RegisteredTypeCount => _typeCache.Count;
    }
}


