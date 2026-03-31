using System;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;

namespace RESTyard.HtoSourceGenerators.TestHtos;

// --- Action parameter types ---

public class MoveParameters : IHypermediaActionParameter
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class SimpleParameters : IHypermediaActionParameter
{
    public int Value { get; set; }
}

// --- Parameterless action ---

public class MarkAsFavoriteOp : HypermediaAction
{
    public MarkAsFavoriteOp(Func<bool> canExecute) : base(canExecute) { }
}

// --- Action with parameter ---

public class CustomerMoveOp : HypermediaAction<MoveParameters>
{
    public CustomerMoveOp(Func<bool> canExecute, MoveParameters? prefilledValues = null)
        : base(canExecute, prefilledValues) { }
}

// --- External action (no parameter) ---

public class ExternalNoParamOp : HypermediaExternalAction
{
    public ExternalNoParamOp(Uri uri, string httpMethod)
        : base(() => true, uri, httpMethod) { }
}

// --- External action (with parameter) ---

public class ExternalWithParamOp : HypermediaExternalAction<SimpleParameters>
{
    public ExternalWithParamOp(Uri uri, string httpMethod, SimpleParameters? prefilled = null)
        : base(() => true, uri, httpMethod, "application/json", prefilled) { }
}

// --- File upload (no parameter) ---

public class FileUploadOp : FileUploadHypermediaAction
{
    public FileUploadOp(Func<bool> canExecute, FileUploadConfiguration? config = null)
        : base(canExecute, config) { }
}

// --- File upload (with parameter) ---

public class FileUploadWithParamOp : FileUploadHypermediaAction<SimpleParameters>
{
    public FileUploadWithParamOp(Func<bool> canExecute, FileUploadConfiguration? config = null, SimpleParameters? prefilled = null)
        : base(canExecute, config, prefilled) { }
}
