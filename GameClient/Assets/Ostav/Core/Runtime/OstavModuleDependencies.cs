using System;
using System.Collections.Generic;

namespace Ostav
{
    public interface IOstavModuleDependency
    {
        string ModuleId { get; }
        string MinimumVersion { get; }
    }
    public interface IOstavDependencyManifest : IOstavModuleManifest
    {
        IReadOnlyCollection<IOstavModuleDependency> Dependencies { get; }
    }
    public sealed class OstavModuleDependency : IOstavModuleDependency
    {
        public OstavModuleDependency(string moduleId,string minimumVersion){if(string.IsNullOrEmpty(moduleId))throw new ArgumentException("Required.","moduleId");if(string.IsNullOrEmpty(minimumVersion))throw new ArgumentException("Required.","minimumVersion");ModuleId=moduleId;MinimumVersion=minimumVersion;}
        public string ModuleId{get;private set;}public string MinimumVersion{get;private set;}
    }
    public sealed class OstavModuleValidationException : InvalidOperationException
    {
        public OstavModuleValidationException(string code,string message):base(message){Code=code;}
        public string Code{get;private set;}
    }
}
