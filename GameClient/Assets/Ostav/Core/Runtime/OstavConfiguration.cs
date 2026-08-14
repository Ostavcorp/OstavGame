using System;
using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavConfiguration { bool TryGet(string configurationNamespace, string key, out string value); }
    public interface IOstavFeatureFlags { bool IsEnabled(string featureNamespace, string flag); }
    public sealed class InMemoryOstavConfiguration : IOstavConfiguration
    {
        private readonly Dictionary<string,string> values=new Dictionary<string,string>(StringComparer.Ordinal);
        public void Set(string n,string key,string value){values[Key(n,key)]=value;}
        public bool TryGet(string n,string key,out string value){return values.TryGetValue(Key(n,key),out value);}
        private static string Key(string n,string key){if(string.IsNullOrEmpty(n)||string.IsNullOrEmpty(key))throw new ArgumentException("Namespace and key are required.");return n+"\n"+key;}
    }
    public sealed class InMemoryOstavFeatureFlags : IOstavFeatureFlags
    {
        private readonly HashSet<string> enabled=new HashSet<string>(StringComparer.Ordinal);
        public void Set(string n,string flag,bool value){var key=Key(n,flag);if(value)enabled.Add(key);else enabled.Remove(key);}
        public bool IsEnabled(string n,string flag){return enabled.Contains(Key(n,flag));}
        private static string Key(string n,string flag){if(string.IsNullOrEmpty(n)||string.IsNullOrEmpty(flag))throw new ArgumentException("Namespace and flag are required.");return n+"\n"+flag;}
    }
}
