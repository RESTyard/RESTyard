#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RESTyard.Client;
using RESTyard.Client.Builder;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Generator.Test.Output;

namespace client._csharp._v3;
public class DefaultHypermediaClientBuilder
{
    public static IHypermediaResolverBuilder CreateBuilder() => HypermediaResolverBuilder.CreateBuilder().ConfigureObjectRegister(register =>
    {
        register.Register<BaseHco>();
        register.Register<ChildHco>();
        register.Register<DerivedHco>();
        register.Register<NoSelfLinkHco>();
        register.Register<QueryHco>();
    });
}

public partial record TP1(string? Property = default);
public partial record TP2(string? Property = default);
public partial record TP3(string? Property = default);
public partial record TP4(string? Property = default);
public partial record TP11(string? Property = default, string? Property2 = default) : TP1(Property);
public partial record TP12(string? Property = default, string? Property2 = default) : TP2(Property);
public partial record TP13(string? Property = default, string? Property2 = default) : TP3(Property);
public partial record TP14(string? Property = default, string? Property2 = default) : TP4(Property);
public partial record WithProperties(string? Property = default, string? KeyProperty = default, string? OptionalProperty = default, string? KeyOptionalProperty = default);
public partial record DerivedWithProperties(string? Property = default, string? KeyProperty = default, string? OptionalProperty = default, string? KeyOptionalProperty = default, bool? DerivedProperty = default) : WithProperties(Property, KeyProperty, OptionalProperty, KeyOptionalProperty);
public partial record QueryHtoQuery(int? SomeInt = default);
[HypermediaClientObject("Base")]
public partial class BaseHco : HypermediaClientObject
{
    public double? Id { get; set; } = default!;

    [Mandatory]
    public List<int> Property { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[] { "self" })]
    public MandatoryHypermediaLink<BaseHco> Self { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[] { "dependency" })]
    public MandatoryHypermediaLink<ChildHco> Dependency { get; set; } = default!;

    [HypermediaRelations(new[] { "dependency2" })]
    public HypermediaLink<ChildHco>? Dependency2 { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[] { "byQuery" })]
    public MandatoryHypermediaLink<QueryHco> ByQuery { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[] { "external" })]
    public MandatoryHypermediaLink<HypermediaClientObject> External { get; set; } = default!;

    [HypermediaRelations(new[] { "item" })]
    public List<ChildHco> Item { get; set; } = default!;

    [HypermediaCommand("Operation")]
    public IHypermediaClientAction? Operation { get; set; }

    [HypermediaCommand("WithParameter")]
    public IHypermediaClientAction<TP2>? WithParameter { get; set; }

    [HypermediaCommand("WithResult")]
    public IHypermediaClientFunction<ChildHco>? WithResult { get; set; }

    [HypermediaCommand("WithParameterAndResult")]
    public IHypermediaClientFunction<ChildHco, External>? WithParameterAndResult { get; set; }

    [HypermediaCommand("Upload")]
    public IHypermediaClientFileUploadAction? Upload { get; set; }

    [HypermediaCommand("UploadWithParameter")]
    public IHypermediaClientFileUploadAction<TP12>? UploadWithParameter { get; set; }
}

[HypermediaClientObject("First", "Second")]
public partial class ChildHco : HypermediaClientObject
{
    [Mandatory]
    [HypermediaRelations(new[] { "self" })]
    public MandatoryHypermediaLink<ChildHco> Self { get; set; } = default!;
}

[HypermediaClientObject("Third")]
public partial class DerivedHco : ChildHco
{
    [Mandatory]
    [HypermediaRelations(new[] { "self" })]
    public new MandatoryHypermediaLink<DerivedHco> Self { get; set; } = default!;
}

[HypermediaClientObject()]
public partial class NoSelfLinkHco : HypermediaClientObject
{
}

[HypermediaClientObject()]
public partial class QueryHco : HypermediaClientObject
{
    public int? NormalKey { get; set; } = default!;
    public string? QueryKey { get; set; } = default!;
    public double? NotAKey { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[] { "self" })]
    public MandatoryHypermediaLink<QueryHco> Self { get; set; } = default!;
}