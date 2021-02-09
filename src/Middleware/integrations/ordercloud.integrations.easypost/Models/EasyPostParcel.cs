using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.easypost
{
    // https://github.com/EasyPost/easypost-csharp/blob/master/EasyPost/Parcel.cs
    public class EasyPostParcel
	{
        public string id { get; set; }
        public string mode { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public double length { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public double weight { get; set; }
        public string predefined_package { get; set; }
    }
}
