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
        [ConfigurationProperty(name: "bpp", DefaultValue = 8)]
        public int Bpp
        {
            get
            {
                return this["bpp"] != null ? (int)this["bpp"] : 8;
            }
            set
            {
                this["bpp"] = value;
            }
        }


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
