using System;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.HtoSourceGenerators.TestHtos;

/// <summary>
/// Covers SirenConverter action code paths:
/// - Parameterless action (CanExecute = true)
/// - Action with parameter + prefilled values
/// - Action with CanExecute = false (should be omitted)
/// - External action (no parameter)
/// - External action (with parameter + prefilled)
/// - File upload (no parameter)
/// - File upload (with parameter)
/// - User-defined action classes
/// </summary>
[HypermediaObject(Title = "Actions Test", Classes = ["ActionsTest"])]
public class HtoWithAllActionTypes : HypermediaObject
{
    [HypermediaAction(Name = "DoNothing", Title = "Parameterless action")]
    public MarkAsFavoriteOp? DoNothing { get; set; }

    [HypermediaAction(Name = "MoveCustomer", Title = "Move a customer", Classes = ["Destructive"])]
    public CustomerMoveOp? MoveCustomer { get; set; }

    [HypermediaAction(Name = "DisabledAction", Title = "Should not appear")]
    public MarkAsFavoriteOp? DisabledAction { get; set; }

    [HypermediaAction(Name = "ExternalNoParam", Title = "External without params")]
    public ExternalNoParamOp? ExternalNoParam { get; set; }

    [HypermediaAction(Name = "ExternalWithParam", Title = "External with params")]
    public ExternalWithParamOp? ExternalWithParam { get; set; }

    [HypermediaAction(Name = "UploadFile", Title = "File upload")]
    public FileUploadOp? UploadFile { get; set; }

    [HypermediaAction(Name = "UploadFileWithParam", Title = "File upload with parameter")]
    public FileUploadWithParamOp? UploadFileWithParam { get; set; }

    [Relations(["self"])]
    public ILink<HtoWithAllActionTypes> Self { get; set; }

    public HtoWithAllActionTypes()
    {
        Self = Link.To(this);
    }
}
