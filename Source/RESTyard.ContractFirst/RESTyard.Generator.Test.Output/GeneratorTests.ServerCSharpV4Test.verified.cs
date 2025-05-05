#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Extensions;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.Generator.Test.Output;

namespace server._csharp._v4;
public static class MimeTypes
{
    public const string APPLICATION_JSON = "application/json";
    public const string APPLICATION_VND_SIREN_JSON = "application/vnd.siren+json";
}

public partial record TP1(string? Property = default);
public partial record TP2(string? Property = default) : IHypermediaActionParameter;
public partial record TP3(string? Property = default) : IHypermediaQuery;
public partial record TP4(string? Property = default) : IHypermediaQuery, IHypermediaActionParameter;
public partial record TP11(string? Property = default, string? Property2 = default) : TP1(Property);
public partial record TP12(string? Property = default, string? Property2 = default) : TP2(Property), IHypermediaActionParameter;
public partial record TP13(string? Property = default, string? Property2 = default) : TP3(Property), IHypermediaQuery;
public partial record TP14(string? Property = default, string? Property2 = default) : TP4(Property), IHypermediaQuery, IHypermediaActionParameter;
public partial record WithProperties(string? Property = default, string? HiddenProperty = default, string? KeyProperty = default, string? OptionalProperty = default, string? HiddenKeyProperty = default, string? HiddenOptionalProperty = default, string? KeyOptionalProperty = default, string? HiddenKeyOptionalProperty = default);
public partial record DerivedWithProperties(string? Property = default, string? HiddenProperty = default, string? KeyProperty = default, string? OptionalProperty = default, string? HiddenKeyProperty = default, string? HiddenOptionalProperty = default, string? KeyOptionalProperty = default, string? HiddenKeyOptionalProperty = default, bool? DerivedProperty = default) : WithProperties(Property, HiddenProperty, KeyProperty, OptionalProperty, HiddenKeyProperty, HiddenOptionalProperty, KeyOptionalProperty, HiddenKeyOptionalProperty);
public partial record QueryHtoQuery(int? SomeInt = default) : IHypermediaQuery;
[HypermediaObject(Title = "A base document", Classes = new string[] { "Base" })]
public partial class BaseHto : HypermediaObject
{
    [Key("id")]
    public double? Id { get; set; }
    public List<int> Property { get; set; }

    [HypermediaAction(Name = "Operation", Title = "Operation")]
    public OperationOp Operation { get; init; }

    [HypermediaAction(Name = "WithParameter", Title = "")]
    public WithParameterOp WithParameter { get; init; }

    [HypermediaAction(Name = "WithResult", Title = "")]
    public WithResultOp WithResult { get; init; }

    [HypermediaAction(Name = "WithParameterAndResult", Title = "")]
    public WithParameterAndResultOp WithParameterAndResult { get; init; }

    [HypermediaAction(Name = "Upload", Title = "")]
    public UploadOp Upload { get; init; }

    [HypermediaAction(Name = "UploadWithParameter", Title = "")]
    public UploadWithParameterOp UploadWithParameter { get; init; }

    public BaseHto(double? id, List<int> property, OperationOp operation, WithParameterOp withParameter, WithResultOp withResult, WithParameterAndResultOp withParameterAndResult, UploadOp upload, UploadWithParameterOp uploadWithParameter, IEnumerable<ChildHto> item, object? dependencyKey, bool hasdependency2, object? dependency2Key, QueryHtoQuery byQueryQuery, object? byQueryKey, HypermediaObjectReferenceBase external) : base(hasSelfLink: true)
    {
        this.Id = id;
        this.Property = property;
        this.Operation = operation;
        this.WithParameter = withParameter;
        this.WithResult = withResult;
        this.WithParameterAndResult = withParameterAndResult;
        this.Upload = upload;
        this.UploadWithParameter = uploadWithParameter;
        Entities.AddRange("item", item);
        Links.Add("dependency", new HypermediaObjectKeyReference(typeof(ChildHto), dependencyKey));
        if (hasdependency2)
        {
            Links.Add("dependency2", new HypermediaObjectKeyReference(typeof(ChildHto), dependency2Key));
        }

        Links.Add("byQuery", new HypermediaObjectQueryReference(typeof(QueryHto), byQueryQuery, byQueryKey));
        Links.Add("external", external);
    }

    public static object CreateKeyObject(double? id) => new
    {
        id = id
    };
    public partial class OperationOp : HypermediaAction
    {
        public OperationOp(Func<bool> canExecuteOperation) : base(canExecuteOperation)
        {
        }
    }

    public partial class WithParameterOp : HypermediaAction<TP2>
    {
        public WithParameterOp(Func<bool> canExecuteWithParameter, TP2? prefilledValues = default) : base(canExecuteWithParameter, prefilledValues)
        {
        }
    }

    public partial class WithResultOp : HypermediaAction
    {
        public WithResultOp(Func<bool> canExecuteWithResult) : base(canExecuteWithResult)
        {
        }
    }

    public partial class WithParameterAndResultOp : HypermediaAction<External>
    {
        public WithParameterAndResultOp(Func<bool> canExecuteWithParameterAndResult, External? prefilledValues = default) : base(canExecuteWithParameterAndResult, prefilledValues)
        {
        }
    }

    public partial class UploadOp : FileUploadHypermediaAction
    {
        public UploadOp(Func<bool> canExecuteUpload, FileUploadConfiguration? fileUploadConfiguration = null) : base(canExecuteUpload, fileUploadConfiguration)
        {
        }
    }

    public partial class UploadWithParameterOp : FileUploadHypermediaAction<TP12>
    {
        public UploadWithParameterOp(Func<bool> canExecuteUploadWithParameter, FileUploadConfiguration? fileUploadConfiguration = null, TP12? prefilledValues = default) : base(canExecuteUploadWithParameter, fileUploadConfiguration, prefilledValues)
        {
        }
    }
}

[HypermediaObject(Title = "", Classes = new string[] { "First", "Second" })]
public partial class ChildHto : HypermediaObject
{
    public ChildHto() : base(hasSelfLink: true)
    {
    }
}

[HypermediaObject(Title = "", Classes = new string[] { "Third" })]
public partial class DerivedHto : ChildHto
{
    public DerivedHto() : base()
    {
    }
}

[HypermediaObject(Title = "", Classes = new string[] { })]
public partial class NoSelfLinkHto : HypermediaObject
{
    public NoSelfLinkHto() : base(hasSelfLink: false)
    {
    }
}

[HypermediaObject(Title = "", Classes = new string[] { })]
public partial class QueryHto : HypermediaQueryResult
{
    [Key("normalKey")]
    public int? NormalKey { get; set; }

    [Key("queryKey")]
    public string? QueryKey { get; set; }
    public double? NotAKey { get; set; }

    public QueryHto(int? normalKey, string? queryKey, double? notAKey, IHypermediaQuery query) : base(query)
    {
        this.NormalKey = normalKey;
        this.QueryKey = queryKey;
        this.NotAKey = notAKey;
    }

    public static object CreateKeyObject(int? normalKey, string? queryKey) => new
    {
        normalKey = normalKey,
        queryKey = queryKey
    };
}