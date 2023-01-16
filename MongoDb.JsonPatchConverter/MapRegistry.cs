using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MongoDb.JsonPatchConverter
{
    public class MapRegistry : IMapRegistry
    {
        private readonly Func<string, Regex> _regexFactory;
        private const string StringMappingNotAllowed = "String mapping is not allowed";
        private readonly ConcurrentDictionary<Type, MapDescription[]> _dictionary;

        public static readonly Func<string, Regex> DefaultRegexFactory = s => new Regex($"^{s}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public MapRegistry() : this(DefaultRegexFactory) { }

        public MapRegistry(Func<string, Regex> regexFactory)
        {
            _regexFactory = regexFactory;
            _dictionary = new ConcurrentDictionary<Type, MapDescription[]>();
        }

        public void MapType<T>() where T : class
        {
            var type = typeof(T);
            if (type == typeof(string))
            {
                throw new InvalidOperationException(StringMappingNotAllowed);
            }

            MapDescription[] ValueFactory(Type t) => t.GetProperties()
                    .SelectMany(_ => CreateTypeMappings(null, false, _.Name, _.PropertyType))
                    .ToArray();

            _dictionary.AddOrUpdate(type, ValueFactory, (a, b) => b);
        }

        public IEnumerable<MapDescription> GetMap(Type t)
        {
            if (false == _dictionary.TryGetValue(t, out var map))
            {
                yield break;
            }
            foreach (var mapDescription in map)
            {
                yield return new MapDescription(mapDescription.Regex, mapDescription.IsIndexer, mapDescription.Type);
            }
        }

        public IEnumerable<MapDescription> GetMap<T>() => GetMap(typeof(T));

        private static IEnumerable<MapDescription> CreateTypeMappings(string path, bool isIndexer, string name, Type type)
        {
            path = string.IsNullOrEmpty(name) ? path : $"{path}/{name}";
            var lst = new List<MapDescription> { new MapDescription(DefaultRegexFactory(path), isIndexer, type) };
            if (type.IsValueType || type == typeof(string))
            {
                return lst;
            }
            if (type.IsArray)
            {
                path += "/[0-9]+";
                var elementType = type.GetElementType(); // 1 к 1
                lst.AddRange(CreateTypeMappings(path, true, string.Empty, elementType));
            }
            else
            {
                var props = type.GetProperties(); // 1 ко многим
                var mapped = props.SelectMany(_ => CreateTypeMappings(path, false, _.Name, _.PropertyType));
                lst.AddRange(mapped);
            }

            return lst;
        }
    }
}