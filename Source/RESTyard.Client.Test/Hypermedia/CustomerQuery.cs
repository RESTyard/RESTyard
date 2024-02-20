using System;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;

namespace RESTyard.Client.Test.Hypermedia
{
    public class CustomerQuery
    {
        public CustomerFilter Filter { get; set; }
        public SortOptions SortBy { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        public int PageSize { get; set; }
        public int PageOffset { get; set; }
    }

    public class SortOptions
    {
        public string PropertyName { get; set; }
        public string SortType { get; set; }
    }

    public class CustomerFilter
    {
        public int MinAge { get; set; }
    }
}