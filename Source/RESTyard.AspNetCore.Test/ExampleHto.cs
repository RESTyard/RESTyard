using System;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace RESTyard.AspNetCore.Test;

[HypermediaObject(Classes = ["Example"])]
public class ExampleHto : IHypermediaObject
{
    [HypermediaAction(Name = "Do something", Title = "Some title")]
    public BasicOp DoSomething { get; set; }
    
    public class BasicOp(Func<bool> canExecute, BasicParameter? prefilledValues = null)
        : HypermediaAction<BasicParameter>(canExecute, prefilledValues);

    public record BasicParameter() : IHypermediaActionParameter;
}