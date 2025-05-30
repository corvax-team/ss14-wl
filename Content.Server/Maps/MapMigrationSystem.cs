using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Server.Maps;

/// <summary>
///     Performs basic map migration operations by listening for engine <see cref="MapLoaderSystem"/> events.
/// </summary>
public sealed class MapMigrationSystem : EntitySystem
{
#pragma warning disable CS0414
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
#pragma warning restore CS0414
    [Dependency] private readonly IResourceManager _resMan = default!;

    private const string MigrationFile = "/migration.yml";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeEntityReadEvent>(OnBeforeReadEvent);

#if DEBUG
        if (!TryReadFile(out var mappings))
            return;

        // Verify that all of the entries map to valid entity prototypes.
        foreach (var node in mappings.Children.Values)
        {
            var newId = ((ValueDataNode) node).Value;
            if (!string.IsNullOrEmpty(newId) && newId != "null")
                DebugTools.Assert(_protoMan.HasIndex<EntityPrototype>(newId), $"{newId} is not an entity prototype.");
        }
#endif
    }

    private bool TryReadFile([NotNullWhen(true)] out MappingDataNode? mappings)
    {
        mappings = null;
        var path = new ResPath(MigrationFile);
        if (!_resMan.TryContentFileRead(path, out var stream))
            return false;

        using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
        var documents = DataNodeParser.ParseYamlStream(reader).FirstOrDefault();

        if (documents == null)
            return false;

        mappings = (MappingDataNode) documents.Root;
        return true;
    }

    private void OnBeforeReadEvent(BeforeEntityReadEvent ev)
    {
        if (!TryReadFile(out var mappings))
            return;

        foreach (var (key, value) in mappings)
        {
            if (value is not ValueDataNode valueNode)
                continue;

            if (string.IsNullOrWhiteSpace(valueNode.Value) || valueNode.Value == "null")
                ev.DeletedPrototypes.Add(key);
            else
                ev.RenamedPrototypes.Add(key, valueNode.Value);
        }
    }
}

//This is too fuck to fix it
// Uncommit, when it's need
//
// using System.Diagnostics.CodeAnalysis;
// using System.IO;
// using System.Linq;
// using Robust.Server.GameObjects;
// using Robust.Server.Maps;
// using Robust.Shared.ContentPack;
// using Robust.Shared.Map.Events;
// using Robust.Shared.Prototypes;
// using Robust.Shared.Serialization.Markdown;
// using Robust.Shared.Serialization.Markdown.Mapping;
// using Robust.Shared.Serialization.Markdown.Value;
// using Robust.Shared.Utility;

// namespace Content.Server.Maps;

// /// <summary>
// ///     Performs basic map migration operations by listening for engine <see cref="MapLoaderSystem"/> events.
// /// </summary>
// public sealed class MapMigrationSystem : EntitySystem
// {
//     [Dependency] private readonly IPrototypeManager _protoMan = default!;
//     [Dependency] private readonly IResourceManager _resMan = default!;

//     private const string MigrationFile = "/migration.yml";

//     //WL-Changes-start
//     private const string WLMigrationFile = "/wl-migration.yml";
//     //WL-Changes-end

//     public override void Initialize()
//     {
//         base.Initialize();
//         SubscribeLocalEvent<BeforeEntityReadEvent>(OnBeforeReadEvent);

// #if DEBUG
//         if (!TryReadFile(out var mappings))
//             return;

//         // Verify that all of the entries map to valid entity prototypes.
//         foreach (var node in mappings.Value.CorvaxMigration.Values)
//         {
//             var newId = ((ValueDataNode) node).Value;
//             if (!string.IsNullOrEmpty(newId) && newId != "null")
//                 DebugTools.Assert(_protoMan.HasIndex<EntityPrototype>(newId), $"{newId} is not an entity prototype.");
//         }

//         //WL-Changes-start
//         // Verify that all of the entries map to valid entity prototypes.
//         foreach (var node in mappings.Value.WLMigration.Values)
//         {
//             var newId = ((ValueDataNode)node).Value;
//             if (!string.IsNullOrEmpty(newId) && newId != "null")
//                 DebugTools.Assert(_protoMan.HasIndex<EntityPrototype>(newId), $"{newId} is not an entity prototype.");
//         }
//         //WL-Changes-end
// #endif
//     }

//     private bool TryReadFile(
//         [NotNullWhen(true)] out /*WL-Changes-start*/(MappingDataNode CorvaxMigration, MappingDataNode WLMigration)?/*WL-Changes-start*/ mappings)
//     {
//         mappings = null;
//         var path = new ResPath(MigrationFile);

//         //WL-Changes-start
//         var wl_path = new ResPath(WLMigrationFile);
//         //WL-Changes-end

//         if (!_resMan.TryContentFileRead(path, out var stream))
//             return false;

//         //WL-Changes-start
//         if (!_resMan.TryContentFileRead(wl_path, out var wl_stream))
//             return false;
//         //WL-Changes-end

//         using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
//         var documents = DataNodeParser.ParseYamlStream(reader).FirstOrDefault();

//         //WL-Changes-start
//         using var wl_reader = new StreamReader(wl_stream, EncodingHelpers.UTF8);
//         var wl_documents = DataNodeParser.ParseYamlStream(wl_reader).FirstOrDefault();
//         //WL-Changes-end

//         if (documents == null /*WL-Changes-start*/|| wl_documents == null/*WL-Changes-end*/)
//             return false;

//         mappings = ((MappingDataNode)documents.Root, (MappingDataNode)wl_documents.Root);
//         return true;
//     }

//     private void OnBeforeReadEvent(BeforeEntityReadEvent ev)
//     {
//         if (!TryReadFile(out var mappings_nullable))
//             return;

//         //WL-Changes-start
//         var dict = new Dictionary<string, string>();

//         var mappings = mappings_nullable.Value;
//         var corvax_mappings = mappings.CorvaxMigration
//             .Select(c_m =>
//             {
//                 if (c_m.Key is not ValueDataNode keyNode || c_m.Value is not ValueDataNode valueNode)
//                     return ((string, string)?)null;

//                 return (keyNode.Value, valueNode.Value);
//             })
//             .Where(c_m => c_m != null);

//         var wl_mappings = mappings.WLMigration
//             .Select(w_m =>
//             {
//                 if (w_m.Key is not ValueDataNode keyNode || w_m.Value is not ValueDataNode valueNode)
//                     return ((string, string)?)null;

//                 return (keyNode.Value, valueNode.Value);
//             })
//             .Where(w_m => w_m != null);

//         // Именно в таком порядке: сначала миграция основы, только потом наша, вдруг наша перезаписывает что-то на основе.
//         foreach (var c_m in corvax_mappings)
//         {
//             if (c_m == null)
//                 continue;

//             var key = c_m.Value.Item1;
//             var value = c_m.Value.Item2;

//             dict[key] = value;
//         }

//         foreach (var w_m in wl_mappings)
//         {
//             if (w_m == null)
//                 continue;

//             var key = w_m.Value.Item1;
//             var value = w_m.Value.Item2;

//             dict[key] = value;
//         }

//         //WL-Changes-end


//         foreach (var (key, value) in /*WL-Changes-start*/dict/*WL-Changes-end*/)
//         {
//             if (string.IsNullOrWhiteSpace(/*WL-Changes-start*/value/*WL-Changes-end*/) || /*WL-Changes-start*/value/*WL-Changes-end*/ == "null")
//                 ev.DeletedPrototypes.Add(/*WL-Changes-start*/key/*WL-Changes-end*/);
//             else
//                 ev.RenamedPrototypes.Add(/*WL-Changes-start*/key/*WL-Changes-end*/, /*WL-Changes-start*/value/*WL-Changes-end*//*WL-Changes-end*/);
//         }
//     }
// }
