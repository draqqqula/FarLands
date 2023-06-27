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
                return Controls[control]();
            }
        }

        /// <summary>
        /// возвращает true в момент нажатия кнопки
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool OnPress(Control control)
        {
            if (this[control] != LastFrameControls[control])
            {
                LastFrameControls[control] = this[control];
                return this[control];
            }
            return false;

        }

        /// <summary>
        /// возвращает true в момент отпускания кнопки
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool OnRelease(Control control)
        {
            if (this[control] != LastFrameControls[control])
            {
                LastFrameControls[control] = this[control];
                return !this[control];
            }
            return false;

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
