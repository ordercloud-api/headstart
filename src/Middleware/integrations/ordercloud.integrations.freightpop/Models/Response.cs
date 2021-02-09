namespace ordercloud.integrations.freightpop
{
    public class Response<TData>
    {
        public TData Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
