using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Animations;
using Microsoft.Xna.Framework;
using VideoGame.Construct;

namespace VideoGame
{
    /// <summary>
    /// слой, на котором могут распологаться объекты, карты тайлов и текст
    /// </summary>
    public class Layer : List<GameObject>
    {
        public readonly string Name;

        public Func<DrawingParameters, GameCamera, DrawingParameters> DrawingFunction { get; set; }

        public readonly double DrawingPriority;

        #region FRAMESTATE
        public Dictionary<GameCamera, FrameState> PointsOfView { get; init; } = new Dictionary<GameCamera, FrameState>();

        public void AddViewer(GameCamera camera)
        {
            PointsOfView.Add(camera, new FrameState());
        }

        public void RemoveViewer(GameCamera camera)
        {
            PointsOfView.Remove(camera);
        }

        private void QuickClear()
        {
            foreach (var view in PointsOfView)
            {
                view.Value.Clear();
            }
        }
        #endregion

        public void UpdatePicture()
        {
            QuickClear();
            foreach (var obj in this)
            {
                if (obj.HasVisual && obj.IsVisible)
                {
                    foreach (var view in PointsOfView)
                    {
                        if (obj.IsVisibleFor(view.Key.Viewer))
                        {
                            view.Value.AddPicture(obj.GetVisualPart(view.Key));
                        }
                    }

                }
            }
        }

        public Layer(string name, Func<DrawingParameters, GameCamera, DrawingParameters> drawingFunction, double drawingPriority)
        {
            DrawingFunction = drawingFunction;
            DrawingPriority = drawingPriority;
            Name = name;
        }
    }

    public class FrameState
    {
        public readonly List<IDisplayable> Pictures = new List<IDisplayable>();

        public void Clear()
        {
            Pictures.Clear();
        }

        public void AddPicture(IDisplayable picture)
        {
            Pictures.Add(picture);
        }
    }
}
