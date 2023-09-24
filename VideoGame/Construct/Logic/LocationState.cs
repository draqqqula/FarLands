using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct;
using static System.Windows.Forms.AxHost;

namespace VideoGame
{
    /// <summary>
    /// описывает текущее состояние локации
    /// </summary>
    public class LocationState : GameState
    {
        public Layer MainLayer;
        public Layer FrontParticlesLayer;
        public Layer BackParticlesLayer;
        public List<Character> Players { get; init; } = new List<Character>();
        public TextObject FPSCounter;
        public TileMap MainTileMap;

        public override void OnTick(TimeSpan deltaTime)
        {
            FPSCounter.Text = Convert.ToString(Math.Round(1/deltaTime.TotalSeconds));
        }

        protected override void OnConnect(GameClient client)
        {
            Character character = new Character(
                this,
                new Vector2(300, 300),
                MainLayer,
                true,
                client,
                MainTileMap,
                new TextObject(Content, "a", "heart", 0, 6f, 3f, Layers["LeftTopBound"],
                new Vector2(30, 30)));
            Players.Add(character);

            var sword = new PrerenderedModel(new Vector2(300, 300), new Point(72, 72), MathF.PI / 6, "sword", ContentStorage);
            MainLayer.PlaceBelow(sword, character);
            character.PickUpWeapon(sword);

            var camera = GetCamera(client);
            camera.SetOuterBorders(MainTileMap.Box);
            camera.LinkTo(character);
        }

        protected override void OnDisconnect(GameClient client)
        {
            var character = Players.Where(it => it.Client == client).FirstOrDefault();
            Players.Remove(character);
            character.Dispose();
        }

        public LocationState() : base(true, true, true, true, true, true)
        {
        }
    }
}
