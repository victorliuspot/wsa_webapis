using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seaspan.hcwpce.sharepoint.Models
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SPFieldAttribute : Attribute
    {
        public string Name { get; set; }
        public string EntityType { get; set; }
    }
}
