using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// содержит информацию об уроне объекта
    /// </summary>
    public class DamageContainer : IBehavior
    {
        public string Name => "DamageContainer";

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        public void Act(TimeSpan deltaTime)
        {
        }

        private Dictionary<string, DamageInstance> Instances;

        public DamageInstance GetDamage(string title)
        {
            return Instances[title];
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public DamageContainer(bool enabled, params (string title, DamageInstance unit)[] instances)
        {
            Enabled = enabled;
            Instances = new Dictionary<string, DamageInstance>();
            foreach (var instance in instances)
            {
                Instances.Add(instance.title, instance.unit);
            }
        }
    }
}
