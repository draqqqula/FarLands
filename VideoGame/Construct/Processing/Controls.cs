using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// одно из действий игрока
    /// </summary>
    public enum Control
    {
        left,
        right,
        jump,
        dash,
        pause
    }
    /// <summary>
    /// содержит информацию о том, какая клавиша за какое действие отвечает
    /// </summary>
    public class GameControls
    {
        private Dictionary<Control, Func<bool>> Controls;
        private Dictionary<Control, bool> LastFrameControls;
        
        /// <summary>
        /// true если соответствующая копка зажата
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool this[Control control]
        {
            get
            {
                if (Controls.ContainsKey(control))
                    return Controls[control]();
                return false;
            }
        }

        public bool OnAny()
        {
            foreach(var control in LastFrameControls)
            {
                if (control.Value != Controls[control.Key]())
                {
                    LastFrameControls[control.Key] = this[control.Key];
                    return true;
                }
            }
            return false;
        }

        public byte GetMap()
        {
            return ConvertBoolArrayToByte(Controls.OrderBy(it => it.Key).Select(it => it.Value()).ToArray());
        }

        private byte ConvertBoolArrayToByte(bool[] boolArray)
        {
            byte result = 0;

            for (int i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i])
                {
                    result |= (byte)(1 << i);
                }
            }

            return result;
        }

        /// <summary>
        /// возвращает true в момент нажатия кнопки
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool OnPress(Control control)
        {
            if (Controls.ContainsKey(control))
            {
                if (this[control] != LastFrameControls[control])
                {
                    LastFrameControls[control] = this[control];
                    return this[control];
                }
                return false;
            }
            else return false;
        }

        /// <summary>
        /// возвращает true в момент отпускания кнопки
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool OnRelease(Control control)
        {
            if (Controls.ContainsKey(control))
            {
                if (this[control] != LastFrameControls[control])
                {
                    LastFrameControls[control] = this[control];
                    return !this[control];
                }
                return false;
            }
            else return false;
        }

        /// <summary>
        /// меняет закреплённую за действием кнопку на эту
        /// </summary>
        /// <param name="control"></param>
        /// <param name="function"></param>
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
