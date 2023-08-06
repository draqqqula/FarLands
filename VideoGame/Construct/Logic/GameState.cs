﻿using Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct;
using Windows.Gaming.Input;

namespace VideoGame
{
    /// <summary>
    /// состояние уровня
    /// </summary>
    public abstract class GameState
    {

        #region HOST

        public ContentManager Content;

        public AnimationBuilder MainAnimationBuilder { get; set; }

        public LevelLoader LevelLoader { get; set; }

        public void Update(TimeSpan deltaTime, bool paused)
        {
            if (IsRemote)
            {
                //...
            }
            else
            {
                if (paused)
                {
                    UpdateCameras(TimeSpan.Zero);
                    UpdatePicture(TimeSpan.Zero);
                    if (LevelLoader.IsReadyToResume)
                        LevelLoader.Resume();
                }
                else
                {
                    UpdateCameras(deltaTime);
                    ExcludeDestroyed();
                        UpdateBehaviors(deltaTime);
                            UpdateObjects(deltaTime);
                                UpdatePicture(deltaTime);

                    LocalUpdate(deltaTime);
                }
            }
        }

        public void AddLayers(params Layer[] layers)
        {
            foreach (var layer in layers)
                Layers.Add(layer.Name, layer);
            Layers = Layers.OrderBy(it => it.Value.DrawingPriority).ToDictionary(it => it.Key, it => it.Value);
        }

        protected GameState(bool isRemote)
        {
            IsRemote = isRemote;
        }

        public abstract void LocalUpdate(TimeSpan deltaTime);

        public Dictionary<string, Layer> Layers { get; set; } = new Dictionary<string, Layer>();

        public List<Sprite> AllObjects { get; init; } = new List<Sprite>();

        #endregion

        #region FAMILIES
        protected ImmutableDictionary<string, Family> Families { get; init; } =
        Family.AllFamilies
        .ToDictionary(it => it.Key, it => it.Value.GetConstructor(new Type[] { }).Invoke(new object[] { }) as Family)
        .ToImmutableDictionary();

        public T GetFamily<T>() where T : Family
        {
            return Families[typeof(T).Name] as T;
        }

        public Family GetFamily(string familyName)
        {
            return Families[familyName];
        }

        #endregion

        #region CLIENT

        #region CONNECTION

        public readonly bool IsRemote;
        protected virtual void OnConnect(GameClient client) { }
            protected virtual void OnDisconnect(GameClient client) { }
            public void Connect(GameClient client)
            {
                if (!IsConnected(client))
                {
                    Clients.Add(client);
                    GameCamera camera = new GameCamera(Vector2.Zero, new Rectangle(-300, -300, 600, 600), client);
                    Cameras.Add(client, camera);
                    foreach (var layer in Layers.Values)
                    {
                        layer.AddViewer(camera);
                    }
                    OnConnect(client);
                }
            }
            public void Disconnect(GameClient client)
            {
                if (IsConnected(client))
                {
                    foreach (var layer in Layers.Values)
                    {
                        layer.RemoveViewer(GetCamera(client));
                    }
                    Clients.Remove(client);
                    Cameras.Remove(client);
                    OnDisconnect(client);
                }
            }
            public bool IsConnected(GameClient client) => Clients.Contains(client);
            protected List<GameClient> Clients { get; init; } = new List<GameClient>();
            #endregion

            #region CAMERA
            protected Dictionary<GameClient, GameCamera> Cameras { get; init; } = new Dictionary<GameClient, GameCamera>();
            public GameCamera GetCamera(GameClient client) => Cameras[client];
            public IEnumerable<GameCamera> AllCameras
            {
                get => Cameras.Values;
            }
            public void UpdateCameras(TimeSpan deltaTime)
            {
                foreach (var camera in Cameras.Values.ToArray())
                {
                    camera.Update(deltaTime);
                }
            }
            #endregion

        #endregion

        #region UPDATES
        private void UpdateObjects(TimeSpan deltaTime)
        {
            foreach (var sprite in AllObjects.ToArray())
            {
                sprite.Update(this, deltaTime * sprite.TimeScale);
            }
        }

        private void UpdatePicture(TimeSpan deltaTime)
        {
            foreach (var layer in Layers.Values)
            {
                layer.UpdatePicture();
            }
        }

        private void UpdateBehaviors(TimeSpan deltaTime)
        {
            foreach (var sprite in AllObjects.ToArray())
            {
                foreach (var behavior in sprite.ActiveBehaviors.Values.ToArray())
                {
                    behavior.Act(deltaTime * sprite.TimeScale);
                }
            }
        }

        private void UpdateFamilies(TimeSpan deltaTime)
        {
            foreach (var family in Families)
            {
                family.Value.CommonUpdate(deltaTime);
            }
        }

        private void ExcludeDestroyed()
        {
            AllObjects.RemoveAll(e => e.Disposed);
            foreach (var layer in Layers.Values)
            {
                foreach (var obj in layer)
                {
                    if (obj.Disposed)
                        layer.Remove(obj);
                }
            }
        }
        #endregion
    }
}
