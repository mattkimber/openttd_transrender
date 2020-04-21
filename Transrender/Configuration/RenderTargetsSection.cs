using System.Configuration;

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
