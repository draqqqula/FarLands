using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public interface IPattern
    {
        public string AnimatorName { get; }
        public string InitialAnimation { get; }
        public Rectangle Hitbox { get; }
        public bool IsHitBoxOnly { get; }
        public List<GameObject> Editions { get; set; }

        public void UpdateMember(GameObject member, IGameState state);

        public GameObject InitializeMember(IGameState state, GameObject member);

        public GameObject CreateCopy(IGameState state, Vector2 position, Layer layer, bool isMirrored)
        {
            if (IsHitBoxOnly) throw new ArgumentException("Extra arguments for HitBoxOnly Pattern");
            var edition = new GameObject(state, AnimatorName, InitialAnimation, Hitbox, position, layer, isMirrored);
            ConnectMember(state, edition);
            return edition;
        }

        public GameObject CreateCopy(IGameState state, Vector2 position)
        {
            var edition = new GameObject(state, Hitbox, position);
            ConnectMember(state, edition);
            return edition;
        }

        private void ConnectMember(IGameState state, GameObject member)
        {
            member = InitializeMember(state, member);
            member.Pattern = this;
            Editions.Add(member);
        }

        public void Update(IGameState state)
        {
            foreach (GameObject member in Editions)
                UpdateMember(member, state);
        }

        public void LinkFamilies(Family[] families)
        {
            foreach (var family in families)
                family.Patterns.Add(this);
        }
    }
}