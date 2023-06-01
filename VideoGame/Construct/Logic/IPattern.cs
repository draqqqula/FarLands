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
        public IGameState State { get; set; }
        public string AnimatorName { get; }
        public string InitialAnimation { get; }
        public Rectangle Hitbox { get; }

        public List<GameObject> Editions { get; set; }

        public void UpdateMember(GameObject member);

        public GameObject InitializeMember(Vector2 position, Layer layer, bool isMirrored);

        public GameObject CreateCopy(Vector2 position, Layer layer, bool isMirrored)
        {
            var edition = InitializeMember(position, layer, isMirrored);
            edition.Pattern = this;
            Editions.Add(edition);
            return edition;
        }

        public void Update()
        {
            foreach (GameObject member in Editions)
                UpdateMember(member);
        }
    }
}