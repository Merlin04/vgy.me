using System.Collections.Generic;

namespace vgy.me.Models
{
    public class Configuration
    {
        public string Userkey { get; set; }
        public List<UploadedFile> UploadedFiles { get; set; }
    }
}