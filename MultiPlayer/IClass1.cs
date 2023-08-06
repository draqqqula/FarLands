using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MultiPlayer
{
    // ПРИМЕЧАНИЕ. Можно использовать команду "Переименовать" в меню "Рефакторинг", чтобы изменить имя интерфейса "IClass1" в коде и файле конфигурации.
    [ServiceContract]
    public interface IClass1
    {
        [OperationContract]
        void DoWork();
    }
}
