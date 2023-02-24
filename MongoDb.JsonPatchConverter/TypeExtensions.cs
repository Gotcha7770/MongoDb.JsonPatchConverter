using System;

namespace MongoDb.JsonPatchConverter
{
    public static class TypeExtensions
    {
        public static bool IsString(this Type type) => type == typeof(string);
        public static bool IsDateTime(this Type type) => type == typeof(DateTime);
        public static bool IsGuid(this Type type) => type == typeof(Guid);

        public static bool SkipTypeProperties(this Type type)
        {
            return type.IsPrimitive 
                   || type.IsEnum 
                   || type.IsString() 
                   || type.IsGuid() 
                   || type.IsDateTime();
        }
    }
}