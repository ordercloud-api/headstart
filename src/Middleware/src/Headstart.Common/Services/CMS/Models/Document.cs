namespace Headstart.Common.Services.CMS.Models
{
    public class Document<T>
    {
        public string ID { get; set; } = string.Empty;

        public T Doc { get; set; }

        public string SchemaSpecUrl { get; set; } = string.Empty;

        public History History { get; set; } = new History();
    }
}