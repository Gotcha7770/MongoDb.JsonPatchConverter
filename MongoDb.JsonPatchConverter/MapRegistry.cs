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

        public static readonly Func<string, Regex> DefaultRegexFactory = s => new Regex($"^{s}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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

            MapType(type);
        }

        private void MapType(Type type)
        {
            MapDescription[] ValueFactory(Type t) => Node.For(t)
                .GetNodes()
                .Expand(x => x.GetNodes())
                .Select(x => ToMapDescription(x, _regexFactory))
                .ToArray();

            _dictionary.AddOrUpdate(type, ValueFactory, (a, b) => b);
        }

        public IEnumerable<MapDescription> GetMap(Type t)
        {
            if (false == _dictionary.TryGetValue(t, out var map))
            {
                yield break;
            }
            foreach (var mapDescription in map) //???
            {
                yield return new MapDescription(mapDescription.Regex, mapDescription.PathType, mapDescription.Type);
            }
        }

        public IEnumerable<MapDescription> GetMap<T>() => GetMap(typeof(T));

        private static MapDescription ToMapDescription(Node node, Func<string, Regex> regexFactory)
        {
            return new MapDescription(regexFactory(node.Path), GetPathType(node), node.Type);
        }

        private static PathType GetPathType(Node node)
        {
            switch (node)
            {
                case ArrayEndNode _:
                    return PathType.EndOfArray;
                case ArrayIndexerNode _:
                    return PathType.Indexer;
                default:
                    return PathType.Field;
            }
        }
    }
}