using System;
using FunicularSwitch.Generators;

namespace RESTyard.Client.Reader;

[ResultType(ErrorType = typeof(HypermediaReaderProblem))]
public abstract partial class HypermediaReaderResult
{
    
}

[UnionType(CaseOrder = CaseOrder.AsDeclared)]
public abstract partial record HypermediaReaderProblem
{
    public sealed record RequiredPropertyMissing_(string Message) : HypermediaReaderProblem;
    public sealed record InvalidFormat_(string Message) : HypermediaReaderProblem;
    public sealed record InvalidClientClass_(string Message) : HypermediaReaderProblem;
    public sealed record Exception_(Exception Exc) : HypermediaReaderProblem;
}