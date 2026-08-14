using System;
using System.Collections.Generic;
using Ostav;

namespace Ostav.Api
{
    public sealed class ExecuteRequestDto : IOstavExternalRequest
    {
        public string ApiVersion { get; set; }
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public string IdentityId { get; set; }
        public string TargetCapabilityId { get; set; }
        public string IntentType { get; set; }
        public string Locale { get; set; }
        public PayloadDto Payload { get; set; }
        IOstavIdentity IOstavExternalRequest.Identity { get { return string.IsNullOrEmpty(IdentityId) ? null : new ApiIdentity(IdentityId); } }
        IOstavPayload IOstavExternalRequest.Payload { get { return Payload; } }
    }

    public sealed class PayloadDto : IOstavPayload
    {
        public string SchemaId { get; set; }
        public string SchemaVersion { get; set; }
        public string ContentType { get; set; }
        public string Data { get; set; }
        public static PayloadDto From(IOstavPayload value)
        { return value == null ? null : new PayloadDto { SchemaId=value.SchemaId,SchemaVersion=value.SchemaVersion,ContentType=value.ContentType,Data=value.Data }; }
    }

    public sealed class ExecuteResponseDto
    {
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public bool Success { get; set; }
        public string Code { get; set; }
        public PayloadDto Payload { get; set; }
        public static ExecuteResponseDto From(IOstavExternalResponse value)
        { return new ExecuteResponseDto{RequestId=value.RequestId,CorrelationId=value.CorrelationId,Success=value.Success,Code=value.Code,Payload=PayloadDto.From(value.Payload)}; }
    }

    public sealed class CapabilityDiscoveryDto
    {
        public string ApiVersion { get; set; }
        public IReadOnlyCollection<ModuleDiscoveryDto> Modules { get; set; }
    }
    public sealed class ModuleDiscoveryDto
    { public string ModuleId{get;set;}public string Version{get;set;}public IReadOnlyCollection<CapabilityDto> Capabilities{get;set;} }
    public sealed class CapabilityDto
    { public string Id{get;set;}public string Version{get;set;}public IReadOnlyCollection<string> SupportedIntentTypes{get;set;} }

    internal sealed class ApiIdentity : IOstavIdentity
    { public ApiIdentity(string id){Id=id;}public string Id{get;private set;}public string IdentityType{get{return "api";}}public string DisplayName{get{return string.Empty;}} }
    internal sealed class ApiExecutionContext : IOstavExecutionContext, IOstavExecutionMetadataContext
    {
        public ApiExecutionContext(ExecuteRequestDto request, IOstavIdentity identity)
        {
            Identity=identity;
            Locale=string.IsNullOrEmpty(request.Locale)?"en":request.Locale;
            Metadata=new OstavExecutionMetadata(request.CorrelationId,request.RequestId,null,"ostav.api",DateTime.UtcNow);
        }
        public IOstavIdentity Identity{get;private set;}public string DeviceId{get{return "api";}}public string Locale{get;private set;}public string SessionId{get{return "http";}}public IOstavExecutionMetadata Metadata{get;private set;}
    }
}
