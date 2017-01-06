namespace WebApiHypermediaExtensionsCore.Util.Repository
{
    public class Pagination
    {
        public Pagination()
        {
        }

        public Pagination(Pagination other)
        {
            PageSize = other.PageSize;
            PageOffset = other.PageOffset;
        }

        public int PageSize { get; set; }

        public int PageOffset { get; set; }

        public bool HasPagination()
        {
            return PageSize > 0;
        }

        public void DisablePagination()
        {
            PageSize = 0;
            PageOffset = 0;
        }
    }
}