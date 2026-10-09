#nullable enable
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.WebApi;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.Generator.Test.Output;
using server._csharp._v7;

namespace server._csharp_controller._v7;
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
    public Task<IActionResult> WithParameterAsync([FromBody] TP2 tP2)
    {
        throw new NotImplementedException();
    }

    [HttpPatch("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.WithResult))]
    public Task<IActionResult> WithResultAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPatch("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.WithParameterAndResult))]
    public Task<IActionResult> WithParameterAndResultAsync([FromBody] External external)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.Upload), System.Net.Mime.MediaTypeNames.Multipart.FormData)]
    public Task<IActionResult> UploadAsync([HypermediaUploadParameterFromForm] HypermediaFileUploadActionParameter parameters)
    {
        throw new NotImplementedException();
    }

    [HttpPost("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.UploadWithParameter), System.Net.Mime.MediaTypeNames.Multipart.FormData)]
    public Task<IActionResult> UploadWithParameterAsync([HypermediaUploadParameterFromForm] HypermediaFileUploadActionParameter<TP12> parameters)
    {
        throw new NotImplementedException();
    }

    [HttpQuery("<stub>"), HypermediaActionEndpoint<BaseHto>(nameof(BaseHto.QueryOperation))]
    public Task<IActionResult> QueryOperationAsync([FromBody] TP4 tP4)
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