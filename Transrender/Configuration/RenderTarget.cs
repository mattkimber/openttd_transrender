using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Configuration
{
    public class RenderTarget : ConfigurationElement
    {
        [ConfigurationProperty(name:"scale")]
        public float Scale
        {
            get
            {
                return (float)this["scale"];
            }
            set
            {
                this["scale"] = value;
            }
        }

        [ConfigurationProperty(name: "folder")]
        public string OutputFolder
        {
            get
            {
                return (string)this["folder"];
            }
            set
            {
                this["folder"] = value;
            }
        }
    }
}
