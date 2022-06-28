using Microsoft.AspNetCore.Http;

namespace Headstart.Common.Models
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; }
    }
}
