using System;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;

namespace RESTyard.AspNetCore.Hypermedia.Actions;

/// <summary>
/// A HypermediaAction which has optional parameters which are determined at runtime.
/// This required a custom schema route which delivers the runtime determine schema. See: <see cref="HttpGetHypermediaActionParameterInfo"/>
/// To find the schema route the <see cref="TParameter"/> is used. If no parameters are required configure so in constructor.
/// <para />
/// It is possible to pass runtime values to the schema route, so the route can have some runtime values to determine teh right schema.
/// When building the route to the schema during serialization the values from a dynamic object will be taken into account.
/// Note that the properties must exactly match the route keys.
/// <para />
/// Prefilled values are of type <see cref="object"/> which will be serialized as default parameters. Anonymous objects can be used too.
/// If a <see cref="string"/> is provided it must contain a valid JSON object which will be embedded in the generated document.
/// Therefor it must fit the serialized end format usually json.
/// Null if none should be provided (default)
/// </summary>
/// <typeparam name="TParameter">Parameter object is used to access a custom schema route. It will not be serialized or used to produce a schema.</typeparam>
public class DynamicHypermediaAction<TParameter> : HypermediaActionBase, IDynamicSchema where TParameter : class, IHypermediaActionParameter
{
    
    private readonly object prefilledValues;

    private readonly bool hasParameters;

    /// <summary>
    /// Create a dynamic action.
    /// <param name="hasParameters">Indicates if the action has a parameter.</param>
    /// <param name="prefilledValues">The action may provide pre filled values which are passed to the client so action parameters can be filled with provided values.</param>
    /// </summary>
    public DynamicHypermediaAction(Func<bool> canExecute, bool hasParameters, object prefilledValues = null) : base(canExecute)
    {
        this.prefilledValues = prefilledValues;
        this.hasParameters = hasParameters;
    }

    /// <summary>
    /// Create a dynamic action.
    /// <param name="hasParameters">Indicates if the action has a parameter.</param>
    /// <param name="prefilledValues">The action may provide pre filled values which are passed to the client so action parameters can be filled with provided values.</param>
    /// </summary>
    public DynamicHypermediaAction(bool hasParameters, object prefilledValues = null) : base(()=>true)
    {
        this.prefilledValues = prefilledValues;
        this.hasParameters = hasParameters;
    }

    public override bool HasParameter() => hasParameters;

    public override object GetPrefilledParameter()
    {
        return prefilledValues;
    }

    public override Type ParameterType()
    {
        return typeof(TParameter);
    }

    /// <summary>
    /// An (dynamic) object containing the route keys to be used to build the route to the schema
    /// </summary>
    public object SchemaRouteKeys { get; set; }
}