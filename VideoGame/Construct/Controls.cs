using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public enum Control
    {
        left,
        right,
        jump,
        dash
    }
    public class GameControls
    {
        private Dictionary<Control, Func<bool>> Controls;
        private Dictionary<Control, bool> LastFrameControls;
        
        public bool this[Control control]
        {
            get
            {
                return Controls[control]();
            }
        }

        public bool OnPress(Control control)
        {
            if (this[control] != LastFrameControls[control])
            {
                LastFrameControls[control] = this[control];
                return this[control];
            }
            return false;

        }

        public bool OnRelease(Control control)
        {
            if (this[control] != LastFrameControls[control])
            {
                LastFrameControls[control] = this[control];
                return !this[control];
            }
            return false;

        }

        public void ChangeControl(Control control, Func<bool> function)
        {
            Controls[control] = function;
            LastFrameControls[control] = function();
        }

        public GameControls()
        {
            Controls = new Dictionary<Control, Func<bool>>();
            LastFrameControls = new Dictionary<Control, bool>();
        }
    }
}
