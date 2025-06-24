#nullable enable
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.WebApi;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using server._csharp._v4;
using RESTyard.Generator.Test.Output;

namespace server._csharp_controller._v4;
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    [HttpGetHypermediaObject("<stub>", typeof(BaseHto))]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPostHypermediaAction("<stub>", typeof(BaseHto.OperationOp))]
    public Task<IActionResult> OperationAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPatchHypermediaAction("<stub>", typeof(BaseHto.WithParameterOp))]
    public Task<IActionResult> WithParameterAsync([HypermediaActionParameterFromBody] TP2 tP2)
    {
        throw new NotImplementedException();
    }

    [HttpPatchHypermediaAction("<stub>", typeof(BaseHto.WithResultOp))]
    public Task<IActionResult> WithResultAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPatchHypermediaAction("<stub>", typeof(BaseHto.WithParameterAndResultOp))]
    public Task<IActionResult> WithParameterAndResultAsync([HypermediaActionParameterFromBody] External external)
    {
        throw new NotImplementedException();
    }

    [HttpDeleteHypermediaAction("<stub>", typeof(BaseHto.UploadOp))]
    public Task<IActionResult> UploadAsync()
    {
        throw new NotImplementedException();
    }

    [HttpPostHypermediaAction("<stub>", typeof(BaseHto.UploadWithParameterOp))]
    public Task<IActionResult> UploadWithParameterAsync([HypermediaActionParameterFromBody] TP12 tP12)
    {
        throw new NotImplementedException();
    }
}

[Route("api/[controller]")]
public class ChildController : ControllerBase
{
    [HttpGetHypermediaObject("<stub>", typeof(ChildHto))]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }
}

[Route("api/[controller]")]
public class DerivedController : ControllerBase
{
    [HttpGetHypermediaObject("<stub>", typeof(DerivedHto))]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }
}

[Route("api/[controller]")]
public class SecondLevelDerivedController : ControllerBase
{
    [HttpGetHypermediaObject("<stub>", typeof(SecondLevelDerivedHto))]
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
    [HttpGetHypermediaObject("<stub>", typeof(QueryHto))]
    public Task<IActionResult> GetAsync()
    {
        throw new NotImplementedException();
    }
}