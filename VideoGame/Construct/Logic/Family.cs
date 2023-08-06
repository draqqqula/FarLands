using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// семья объектов
    /// позволяет вручную сгруппировать ряд объектов
    /// </summary>
    public abstract class Family : IEnumerable<Sprite>
    {
        public static readonly ImmutableDictionary<string, Type> AllFamilies =
            Assembly.GetAssembly(typeof(Family))
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(Family)))
            .ToDictionary(it => it.Name, it => it).ToImmutableDictionary();

        protected List<Sprite> Members = new List<Sprite>();

        public virtual void CommonUpdate(TimeSpan deltaTime)
        {
        }

        public virtual void Initialize(Sprite member)
        {
        }

        public virtual void OnReplenishment(Sprite member)
        {
        }

        public virtual void OnAbandonment(Sprite member)
        {
        }

        public void AddMember(Sprite member)
        {
            Members.Add(member);
            OnReplenishment(member);
        }

        public void RemoveMember(Sprite member)
        {
            Members.Remove(member);
            OnAbandonment(member);
        }

        IEnumerator<Sprite> IEnumerable<Sprite>.GetEnumerator()
        {
            return Members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Members.GetEnumerator();
        }
    }
}
