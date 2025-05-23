using System.Collections.Immutable;
using FunicularSwitch.Generators;
using RESTyard.Client.Exceptions;

namespace RESTyard.Client.Resolver;

[ResultType(ErrorType = typeof(HypermediaProblem))]
public abstract partial class HypermediaResult
{
    
}

[UnionType(CaseOrder = CaseOrder.AsDeclared)]
public abstract partial record HypermediaProblem
{
    public sealed record ProblemDetails_(ProblemDetails Details) : HypermediaProblem;
    public sealed record StatusCode_(int? Status) : HypermediaProblem;
    public sealed record InvalidRequest_(string Message) : HypermediaProblem;
    public sealed record InvalidResponse_(string Message) : HypermediaProblem;
    public sealed record BadHcoDefinition_(string Message) : HypermediaProblem;
    public sealed record Exception_(System.Exception Exc) : HypermediaProblem;
    public sealed record Multiple_(IImmutableList<HypermediaProblem> Problems) : HypermediaProblem;
}

public static class HypermediaResultMerge
{
    [MergeError]
    public static HypermediaProblem Merge(this HypermediaProblem problem, HypermediaProblem other)
    {
        if (problem is HypermediaProblem.Multiple_ multiple)
        {
            if (other is HypermediaProblem.Multiple_ otherMultiple)
            {
                return HypermediaProblem.Multiple(multiple.Problems.AddRange(otherMultiple.Problems));
            }
            else
            {
                return HypermediaProblem.Multiple(multiple.Problems.Add(other));
            }
        }
        else if (other is HypermediaProblem.Multiple_ otherMultiple)
        {
            return HypermediaProblem.Multiple(otherMultiple.Problems.Insert(0, problem));
        }
        else
        {
            return HypermediaProblem.Multiple(ImmutableList.Create(problem, other));
        }
    }
}