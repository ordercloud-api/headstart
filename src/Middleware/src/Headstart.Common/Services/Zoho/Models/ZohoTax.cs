using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoTax
    {
        /// <summary>
        /// Gets or sets the tax_name.
        /// </summary>
        /// <value>The tax_name.</value>
        public string tax_name { get; set; }
        /// <summary>
        /// Gets or sets the tax_amount.
        /// </summary>
        /// <value>The tax_amount.</value>
        public double tax_amount { get; set; }
        /// <summary>
        /// Gets or sets the tax_id.
        /// </summary>
        /// <value>The tax_id.</value>
        public string tax_id { get; set; }
        /// <summary>
        /// Gets or sets the tax_percentage.
        /// </summary>
        /// <value>The tax_percentage.</value>
        public double tax_percentage { get; set; }
        /// <summary>
        /// Gets or sets the tax_type.
        /// </summary>
        /// <value>The tax_type.</value>
        public string tax_type { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ZohoTax" /> is is_compound.
        /// </summary>
        /// <value><c>true</c> if is_compound; otherwise, <c>false</c>.</value>
        public bool is_compound { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ZohoTax"/> is is_default_tax.
        /// </summary>
        /// <value><c>true</c> if is_default_tax; otherwise, <c>false</c>.</value>
        public bool is_default_tax { get; set; }
    }
}
