using System;
using System.Collections.Generic;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;

namespace Benchmarking.Hcos
{
    [HypermediaClientObject("HighlightReport")]
    public class HighlightReportHco : HypermediaClientObject
    {
        public DateTimeOffset Date { get; set; }

        public string Text { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}