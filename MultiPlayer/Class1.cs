using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MultiPlayer
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Class1" в коде и файле конфигурации.
    [ServiceBehavior]
    public class Class1 : IClass1
    {
        public void DoWork()
        {
        }
    }
}
