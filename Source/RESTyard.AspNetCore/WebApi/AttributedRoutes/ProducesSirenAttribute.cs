using Microsoft.AspNetCore.Mvc;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

public class ProducesSirenAttribute<TSiren> : ProducesAttribute<TSiren>
{
    public ProducesSirenAttribute()
    {
        this.ContentTypes = [DefaultMediaTypes.Siren];
    }
}