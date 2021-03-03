namespace Headstart.Common.Services.CMS
{
    public class ListArgsPageOnly
    {
        public ListArgsPageOnly()
        {
            Page = 1;
            PageSize = 20;
        }

        public ListArgsPageOnly(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}