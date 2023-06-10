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

        public List<GameObject> Editions { get; set; }

        public void UpdateMember(GameObject member, IGameState state);

        public GameObject InitializeMember(IGameState state, GameObject member);

        public GameObject CreateCopy(IGameState state, Vector2 position, Layer layer, bool isMirrored)
        {

            var edition = new GameObject(state, AnimatorName, InitialAnimation, Hitbox, position, layer, isMirrored);
            edition = InitializeMember(state, edition);
            edition.Pattern = this;
            Editions.Add(edition);
            return edition;
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