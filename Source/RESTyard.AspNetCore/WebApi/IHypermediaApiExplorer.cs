using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace RESTyard.AspNetCore;

public interface IHypermediaApiExplorer
{
    IReadOnlyCollection<string> GetFullRouteTemplateFor(Type type);

    IReadOnlyCollection<ApiDescription> GetHypermediaEndpoints();
}