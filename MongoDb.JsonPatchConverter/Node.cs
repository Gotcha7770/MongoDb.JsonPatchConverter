using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoDb.JsonPatchConverter
{
    internal class Node
    {
        internal Node(string path, Type type)
        {
            Path = path;
            Type = type;
        }

        public string Path { get; }
        public Type Type { get; }

        public virtual IEnumerable<Node> GetNodes()
        {
            return GetProperties(Type).Select(x => Create(Path, x.Name, x.PropertyType));
        }

        public static Node For<T>() => For(typeof(T));

        public static Node For(Type type) => new Node(string.Empty, type);

        public static Node Create(string path, string name, Type type)
        {
            path = $"{path}/{name}";

            return type.IsArray 
                ? new ArrayNode(path, type) 
                : new Node(path, type);
        }

        internal static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
                return Enumerable.Empty<PropertyInfo>();
            if (type.IsArray)
                return type.GetElementType()?.GetProperties() ?? Enumerable.Empty<PropertyInfo>();

            return type.GetProperties();
        }
    }

    internal class ArrayNode : Node
    {
        internal ArrayNode(string path, Type type) : base(path, type) { }
        
        public override IEnumerable<Node> GetNodes()
        {
            var elementType = Type.GetElementType();
            yield return new ArrayEndNode($"{Path}/-", elementType);
            yield return new ArrayIndexerNode($"{Path}/[0-9]+", elementType);
        }
    }

    internal class ArrayEndNode : Node
    {
        internal ArrayEndNode(string path, Type type) : base(path, type) { }

        public override IEnumerable<Node> GetNodes() => Enumerable.Empty<Node>();
    }

    internal class ArrayIndexerNode : Node
    {
        internal ArrayIndexerNode(string path, Type type) : base(path, type) { }
    }
}