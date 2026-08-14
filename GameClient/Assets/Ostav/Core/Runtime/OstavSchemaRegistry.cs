using System;
using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavSchemaDescriptor
    {
        string SchemaId { get; }
        string Version { get; }
        OstavDataClassification Classification { get; }
    }
    public interface IOstavSchemaRegistry
    {
        void Register(IOstavSchemaDescriptor descriptor);
        bool TryGet(string schemaId, string version, out IOstavSchemaDescriptor descriptor);
    }
    public sealed class OstavSchemaDescriptor : IOstavSchemaDescriptor
    {
        public OstavSchemaDescriptor(string schemaId,string version,OstavDataClassification classification)
        {if(string.IsNullOrEmpty(schemaId))throw new ArgumentException("Required.","schemaId");if(string.IsNullOrEmpty(version))throw new ArgumentException("Required.","version");SchemaId=schemaId;Version=version;Classification=classification;}
        public string SchemaId{get;private set;} public string Version{get;private set;} public OstavDataClassification Classification{get;private set;}
    }
    public sealed class OstavSchemaRegistry : IOstavSchemaRegistry
    {
        private readonly Dictionary<string,IOstavSchemaDescriptor> values=new Dictionary<string,IOstavSchemaDescriptor>(StringComparer.Ordinal);
        public void Register(IOstavSchemaDescriptor descriptor){if(descriptor==null)throw new ArgumentNullException("descriptor");var key=Key(descriptor.SchemaId,descriptor.Version);if(values.ContainsKey(key))throw new InvalidOperationException("Schema is already registered.");values.Add(key,descriptor);}
        public bool TryGet(string schemaId,string version,out IOstavSchemaDescriptor descriptor){return values.TryGetValue(Key(schemaId,version),out descriptor);}
        private static string Key(string id,string version){if(string.IsNullOrEmpty(id)||string.IsNullOrEmpty(version))throw new ArgumentException("Schema id and version are required.");return id+"\n"+version;}
    }
}
