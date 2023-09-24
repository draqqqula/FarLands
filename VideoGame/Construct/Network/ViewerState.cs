using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace VideoGame
{
    public class ViewerState : GameState
    {
        public class ByteKeyEqualityComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                return x.SequenceEqual(y);
            }


            public int GetHashCode([DisallowNull] byte[] obj)
            {
                int result = 17;
                for (int i = 0; i < obj.Length; i++)
                {
                    unchecked
                    {
                        result = result * 23 + obj[i];
                    }
                }
                return result;
            }
        }

        public Dictionary<byte[], GameObject> NetworkCollection = new(new ByteKeyEqualityComparer());
        public Layer mainLayer = new Layer("Main", (pos, cam) => pos, 1);
        private IPAddress Adress;
        private int Port;
        private IPEndPoint OpenAdress;
        public UdpClient Reciever;

        private byte[] CurrentData;

        public ViewerState(ContentStorage contentStorage, string IP) : base(false, false, false, false, false, false)
        {
            Layers.Add("main", mainLayer);
            ContentStorage = contentStorage;
            Adress = IPAddress.Parse(Regex.Split(IP, ":")[0]);
            Port = int.Parse(Regex.Split(IP, ":")[1]);
            OpenAdress = IPEndPoint.Parse(IP);
            Reciever = new UdpClient();
        }

        private void SendInitialClientInfo(GameClient client)
        {
            List<byte> buffer = new ();
            buffer.AddRange(BitConverter.GetBytes(client.Window.X));
            buffer.AddRange(BitConverter.GetBytes(client.Window.Y));
            buffer.AddRange(BitConverter.GetBytes(client.Window.Width));
            buffer.AddRange(BitConverter.GetBytes(client.Window.Height));
            Reciever.SendAsync(buffer.ToArray(), OpenAdress);
        }

        Task ConnectionLoop;
        private void RecieveInLoop()
        {
            while (DoRecieveData)
            {
                CurrentData = GetStreamData();
            }
        }

        private void BringState()
        {
            var message = Reciever.ReceiveAsync().Result;
            Port = message.RemoteEndPoint.Port;
            foreach (var map in UnpackTileMaps(message.Buffer, mainLayer))
            {
                NetworkCollection.Add(map.LinkedID, map);
            }
        }

        private byte[] GetStreamData()
        {
            return Reciever.Receive(ref OpenAdress);
        }

        public override void OnTick(TimeSpan deltaTime)
        {
                if (CurrentData is not null)
                {
                    foreach (var camera in Cameras.Values)
                    {
                        mainLayer.PointsOfView[camera].Clear();

                        foreach (var picture in Unpack(CurrentData, ContentStorage, NetworkCollection))
                        {
                            mainLayer.PointsOfView[camera].AddPicture(picture);
                        }
                    }
                foreach (var client in Clients)
                {
                    if (client.Controls.OnAny())
                    {
                        var map = client.Controls.GetMap();
                        Reciever.Send(new byte[1] { map }, new IPEndPoint(Adress, Port));
                    }
                }
                }
        }

        private bool DoRecieveData = false;
        protected override void OnConnect(GameClient client)
        {
            SendInitialClientInfo(client);
            BringState();
            DoRecieveData = true;
            ConnectionLoop = new Task(RecieveInLoop);
            ConnectionLoop.Start();
        }
    }
}
