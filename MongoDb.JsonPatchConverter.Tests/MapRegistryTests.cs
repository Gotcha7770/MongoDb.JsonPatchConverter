using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aqua.Dynamic;
using FluentAssertions;
using MongoDb.JsonPatchConverter.Tests.TestClasses;
using Xunit;

namespace MongoDb.JsonPatchConverter.Tests;

public class MapRegistryTests
{
    [Fact]
    public void ExpandProperties()
    {
        IEnumerable<PropertyInfo> GetProperties(PropertyInfo propertyInfo) => Node.GetProperties(propertyInfo.PropertyType);

        var properties = typeof(User).GetProperties()
            .Expand(GetProperties)
            .Select(x => (x.Name, x.PropertyType));

        properties.Should()
            .BeEquivalentTo(new[]
            {
                ("Id", typeof(Guid)),
                ("Name", typeof(string)),
                ("Rating", typeof(double)),
                ("TotalPets", typeof(int)),
                ("Dogs", typeof(Dog[])),
                ("Name", typeof(string)),
                ("Age", typeof(int)),
                ("FavoriteFood", typeof(string)),
                ("Legs", typeof(Leg[])),
                ("IsOk", typeof(bool)),
            });
    }

    [Fact]
    public void ExpandNodes()
    {
        var nodes = Node.For<User>().GetNodes()
            .Expand(x => x.GetNodes());

        nodes.Should()
            .BeEquivalentTo(new[]
            {
                new Node("/Id", typeof(Guid)),
                new Node("/Name", typeof(string)),
                new Node("/Rating", typeof(double)),
                new Node("/TotalPets", typeof(int)),
                new Node("/Dogs", typeof(Dog[])),
                new Node("/Dogs/-", typeof(Dog)),
                new Node("/Dogs/[0-9]+", typeof(Dog)),
                new Node("/Dogs/[0-9]+/Name", typeof(string)),
                new Node("/Dogs/[0-9]+/Age", typeof(int)),
                new Node("/Dogs/[0-9]+/FavoriteFood", typeof(string)),
                new Node("/Dogs/[0-9]+/Legs", typeof(Leg[])),
                new Node("/Dogs/[0-9]+/Legs/-", typeof(Leg)),
                new Node("/Dogs/[0-9]+/Legs/[0-9]+", typeof(Leg)),
                new Node("/Dogs/[0-9]+/Legs/[0-9]+/IsOk", typeof(bool))
            });
    }

    [Fact]
    public void MapType()
    {
        var registry = new MapRegistry();
        registry.MapType<User>();

        var mapDescriptions = registry.GetMap<User>();

        mapDescriptions.Should()
            .BeEquivalentTo(new[]
            {
                new MapDescription(MapRegistry.DefaultRegexFactory("/Id"), PathType.Field, typeof(Guid)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Name"), PathType.Field, typeof(string)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Rating"), PathType.Field, typeof(double)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/TotalPets"), PathType.Field, typeof(int)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs"), PathType.Field, typeof(Dog[])),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/-"), PathType.EndOfArray, typeof(Dog)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+"), PathType.Indexer, typeof(Dog)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+/Name"), PathType.Field, typeof(string)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+/Age"), PathType.Field, typeof(int)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+/FavoriteFood"), PathType.Field, typeof(string)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+/Legs"), PathType.Field, typeof(Leg[])),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+/Legs/-"), PathType.EndOfArray, typeof(Leg)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+/Legs/[0-9]+"), PathType.Indexer, typeof(Leg)),
                new MapDescription(MapRegistry.DefaultRegexFactory("/Dogs/[0-9]+/Legs/[0-9]+/IsOk"), PathType.Field, typeof(bool)),
            });
    }

    [Fact]
    public void MapToDynamicObject()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John",
            TotalPets = 1,
            Dogs = new[]
            {
                new Dog
                {
                    Name = "Kuchubey",
                    Legs = new[]
                    {
                        new Leg(),
                        new Leg(),
                        new Leg(),
                        new Leg()
                    }
                }
            }
        };
        
        var dynamicObject = new DynamicObjectMapper().MapObject(user);
    }
}