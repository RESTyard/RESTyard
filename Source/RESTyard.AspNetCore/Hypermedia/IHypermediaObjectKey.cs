using System.Collections.Generic;

namespace RESTyard.AspNetCore.Hypermedia;

public interface IHypermediaObjectKey<out THto> : IEnumerable<KeyValuePair<string, object?>>
    where THto : IHypermediaObject;