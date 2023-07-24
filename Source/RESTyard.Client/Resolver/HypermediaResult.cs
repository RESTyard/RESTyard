using System.Net;
using FunicularSwitch.Generators;
using RESTyard.Client.Exceptions;
using RESTyard.Client.Hypermedia;

namespace RESTyard.Client.Resolver;

[ResultType(ErrorType = typeof(HypermediaProblem))]
public abstract partial class HypermediaResult
{
    
}

[UnionType(CaseOrder = CaseOrder.AsDeclared)]
public abstract record HypermediaProblem
{
    public static HypermediaProblem ProblemDetails(ProblemDetails pd) => new ProblemDetails_(pd);
    public static HypermediaProblem StatusCode(int? status, string message) => new StatusCode_(status, message);
    public static HypermediaProblem InvalidRequest(string message) => new InvalidRequest_(message);
    public static HypermediaProblem InvalidResponse(string message) => new InvalidResponse_(message);
    public static HypermediaProblem BadHcoDefinition(string message) => new BadHcoDefinition_(message);
    public static HypermediaProblem Exception(System.Exception exception) => new Exception_(exception);

    public sealed record ProblemDetails_(ProblemDetails Details) : HypermediaProblem;
    public sealed record StatusCode_(int? Status, string Message) : HypermediaProblem;
    public sealed record InvalidRequest_(string Message) : HypermediaProblem;
    public sealed record InvalidResponse_(string Message) : HypermediaProblem;
    public sealed record BadHcoDefinition_(string Message) : HypermediaProblem;
    public sealed record Exception_(System.Exception Exc) : HypermediaProblem;
}