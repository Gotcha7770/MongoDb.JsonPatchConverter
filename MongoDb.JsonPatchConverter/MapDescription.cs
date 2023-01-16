using System;
using System.Text.RegularExpressions;

namespace MongoDb.JsonPatchConverter
{
    public enum PathType
    {
        Field,
        EndOfArray,
        Indexer
    }

    public class MapDescription : IEquatable<MapDescription>
    {
        public MapDescription(Regex regex, PathType pathType, Type type)
        {
            Regex = regex;
            PathType = pathType;
            Type = type;
        }

        public Regex Regex { get; }
        public PathType PathType { get; }
        public Type Type { get; }

        public bool Equals(MapDescription other)
        {
            return other != null 
                   && Regex.ToString() == other.Regex.ToString()
                   && PathType == other.PathType
                   && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Regex.ToString().GetHashCode() ^ PathType.GetHashCode() ^ Type.FullName.GetHashCode();
        }
    }
}