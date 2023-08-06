using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace VideoGame
{
    public class Template<T> where T : Sprite
    {
        private T Original;
        public Template(T original)
        {
            Original = original;
        }

        public Sprite CreateCopy()
        {
            throw new NotImplementedException();
        }
    }
}
