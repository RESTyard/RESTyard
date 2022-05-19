﻿using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;

namespace Benchmarking.Hcos
{
    [HypermediaClientObject("Score")]
    public class ScoreHco : HypermediaClientObject
    {
        public int TotalScore { get; set; }

        public int DailyScore { get; set; }

        public bool IsCurrent { get; set; }

        public bool ShowTotal { get; set; }
    }
}