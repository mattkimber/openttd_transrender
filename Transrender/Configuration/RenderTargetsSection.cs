using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Configuration
{
    public class RenderTargetsSection : ConfigurationSection
    {
        [ConfigurationProperty(name: "targets", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ConfigurationElementCollection<RenderTarget>),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ConfigurationElementCollection<RenderTarget> Targets
        {
            get
            {
                return (ConfigurationElementCollection<RenderTarget>)this["targets"];
            }
            set
            {
                this["targets"] = value;
            }
        }
    }
}
