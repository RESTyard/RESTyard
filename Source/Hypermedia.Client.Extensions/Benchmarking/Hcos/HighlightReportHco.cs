using System;
using System.Collections.Generic;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;

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