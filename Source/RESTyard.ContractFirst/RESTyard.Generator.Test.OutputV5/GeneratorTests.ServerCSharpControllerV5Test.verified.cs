#nullable enable
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.WebApi;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using server._csharp._v5;
using RESTyard.Generator.Test.Output;

namespace server._csharp_controller._v5;
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    [HttpGet("<stub>"), HypermediaObjectEndpoint<BaseHto>]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPost("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.Operation))]
    public Task<IActionResult> OperationAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPatch("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.WithParameter))]
    public Task<IActionResult> WithParameterAsync([HypermediaActionParameterFromBody] TP2 tP2)
    {
        throw new NotImplementedException();
    }

    [HttpPatch("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.WithResult))]
    public Task<IActionResult> WithResultAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPatch("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.WithParameterAndResult))]
    public Task<IActionResult> WithParameterAndResultAsync([HypermediaActionParameterFromBody] External external)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.Upload))]
    public Task<IActionResult> UploadAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPost("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.UploadWithParameter))]
    public Task<IActionResult> UploadWithParameterAsync([HypermediaActionParameterFromBody] TP12 tP12)
    {
        throw new NotImplementedException();
    }
}

[Route("api/[controller]")]
public class ChildController : ControllerBase
{
    [HttpGet("<stub>"), HypermediaObjectEndpoint<ChildHto>]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }
}

[Route("api/[controller]")]
public class DerivedController : ControllerBase
{
    [HttpGet("<stub>"), HypermediaObjectEndpoint<DerivedHto>]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }
}

[Route("api/[controller]")]
public class SecondLevelDerivedController : ControllerBase
{
    [HttpGet("<stub>"), HypermediaObjectEndpoint<SecondLevelDerivedHto>]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }
}

[Route("api/[controller]")]
public class NoSelfLinkController : ControllerBase
{
}

[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    [HttpGet("<stub>"), HypermediaObjectEndpoint<QueryHto>]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }
}