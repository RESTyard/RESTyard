using FunicularSwitch.Generators;
using RESTyard.Client.Hypermedia;

namespace RESTyard.Client.Resolver;

[UnionType(CaseOrder = CaseOrder.AsDeclared)]
public abstract partial record LinkOrEntity<TResultType>
    where TResultType : HypermediaClientObject
{
    public sealed record Link_(MandatoryHypermediaLink<TResultType> Value) : LinkOrEntity<TResultType>;

    public sealed record Entity_(TResultType Value) : LinkOrEntity<TResultType>;
}
