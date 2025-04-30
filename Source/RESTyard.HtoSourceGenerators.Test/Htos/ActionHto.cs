using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.HtoSourceGenerators.Attributes;

namespace RESTyard.HtoSourceGenerators.Test.Htos;

[HypermediaObject(Title = "Only contains actions")]
public class ActionHto
{
    [HypermediaAction(Name = "ReCalc", Title = "My text")]
    public HypermediaAction ReCalculateOp { get; set; } // how to find route? how to assign?
    
    // with parameters
    [HypermediaAction(Name = "Rename", Title = "Rename this.")]
    public HypermediaAction<RenameJobParameters> RenameOp { get; set; } // works without derived type
    
    // todo external/internal-unmapped action
    // dynamic action
    // file upload
    // specify default in assignment
    // add [Mandatory] to indicate this action is always available independent of state and authorization

    // in controllers:
    // could work like this: [HypermediaAction(HtoAction = nameof(ActionHto.ReCalculateOp), HtoClass = nameof(ActionHto))]
    // -> type save, navigatable, refactor save
    // -> works for parameterless actions
    // -> for parameter actions not needed since we coudl match by parameter name, BUT enables parameter reuse! 
    // how to make actiosn that produce a link indicate the result link type e.g. generate Car query will return a CarQueryResultHto link.
    
    // for get routes:
    // [HypermediaAccessTo<ActionHto>]??, better use [ProducesResponseType(...)] / ActionResult<Hto> and modify ApiExplorer so OpenApi is correct
}

public record RenameJobParameters // or class
    : IHypermediaActionParameter
{
    public string NewName { get; set; }
    
    public string? OptionalLastName { get; set; }
}







