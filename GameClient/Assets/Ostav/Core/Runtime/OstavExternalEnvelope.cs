using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavExternalRequest
    {
        string ApiVersion{get;} string RequestId{get;} string CorrelationId{get;}
        IOstavIdentity Identity{get;} string TargetCapabilityId{get;} string IntentType{get;}
        string Locale{get;} IOstavPayload Payload{get;}
    }
    public interface IOstavExternalResponse
    {
        string RequestId{get;} string CorrelationId{get;} bool Success{get;}
        string Code{get;} IOstavPayload Payload{get;}
    }
    public sealed class OstavExternalResponse : IOstavExternalResponse
    {
        public OstavExternalResponse(string requestId,string correlationId,IOstavActionResult result){RequestId=requestId;CorrelationId=correlationId;Success=result.Success;Code=result.Code;Payload=result.Payload;}
        public string RequestId{get;private set;}public string CorrelationId{get;private set;}public bool Success{get;private set;}public string Code{get;private set;}public IOstavPayload Payload{get;private set;}
    }
    public sealed class OstavExternalRequestAdapter
    {
        private readonly IOstavPlatformExecutor executor; private readonly IOstavClock clock;
        public OstavExternalRequestAdapter(IOstavPlatformExecutor executor,IOstavClock clock){this.executor=executor??throw new ArgumentNullException("executor");this.clock=clock??throw new ArgumentNullException("clock");}
        public async Task<IOstavExternalResponse> ExecuteAsync(IOstavExternalRequest request,IOstavExecutionContext context,CancellationToken token)
        {if(request==null)throw new ArgumentNullException("request");if(context==null)throw new ArgumentNullException("context");if(string.IsNullOrEmpty(request.RequestId)||string.IsNullOrEmpty(request.CorrelationId)||string.IsNullOrEmpty(request.TargetCapabilityId)||string.IsNullOrEmpty(request.IntentType))return new OstavExternalResponse(request.RequestId,request.CorrelationId,new OstavActionResult(false,OstavPlatformCodes.InvalidRequest,"Invalid request."));string locale=string.IsNullOrWhiteSpace(request.Locale)?context.Locale:null;locale=string.IsNullOrWhiteSpace(locale)?"en":locale;var intent=new OstavIntent(request.IntentType,request.TargetCapabilityId,locale,clock.UtcNow,request.Payload);var result=await executor.ExecuteIntentAsync(intent,context,token);return new OstavExternalResponse(request.RequestId,request.CorrelationId,result);}
    }
}
