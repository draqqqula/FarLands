using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// Шаблон объекта, который позволяет перейти с уровня на уровень.
    /// </summary>
    public class Gates : IPattern
    {
        public string AnimatorName => null;

        public string InitialAnimation => null;

        public Rectangle Hitbox => new Rectangle(-100, -100, 200, 200);

        public bool IsHitBoxOnly => true;

        public List<GameObject> Editions { get; set; }

        private string DestinationLevelName;
        private GameObject Player;

        public GameObject InitializeMember(IGameState state, GameObject member)
        {
            return member;
        }

        public void UpdateMember(GameObject member, IGameState state)
        {
            if (Player.Layout.Intersects(member.Layout))
                state.World.LoadLevel(DestinationLevelName);
        }

        public Gates(string destinationLevelName, GameObject player)
        {
            DestinationLevelName = destinationLevelName;
            Player = player;
            Editions = new List<GameObject>();
        }
    }
}
