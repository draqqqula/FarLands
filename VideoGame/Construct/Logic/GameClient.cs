using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;

namespace VideoGame
{
    public class GameClient
    {
        public enum GameLanguage
        {
            Russian,
            English
        }

        public readonly GameLanguage Language;
        public readonly GameControls Controls;
        public Rectangle Window { get; set; }
        public readonly bool IsRemote;
        private readonly UdpClient Sender;
        private readonly IPEndPoint RemoteHost;

        #region CONSTRUCTORS
        private GameClient(Rectangle window, GameControls controls, GameLanguage language, bool isRemote, IPEndPoint remoteHost)
        {
            Window = window;
            Controls = controls;
            Language = language;
            IsRemote = isRemote;
            if (IsRemote)
            {
                Sender = new UdpClient(0);
                RemoteHost = remoteHost;
                recieveTask = StartWaitForData();
                CreateRemoteControls();
            }
        }

        public GameClient(Rectangle window, GameControls controls, GameLanguage language) :
        this(window, controls, language, false, null)
        {
        }

        public GameClient(Rectangle window, GameControls controls, GameLanguage language, IPEndPoint remoteHost) :
        this(window, controls, language, true, remoteHost)
        {
        }

        public GameClient(Rectangle window, GameControls controls, GameLanguage language, string adress) :
        this(window, controls, language, true, IPEndPoint.Parse(adress))
        {
        }
        #endregion

        #region NETWORK
        public void SendData(byte[] data)
        {
            if (IsRemote)
                Sender.SendAsync(data, RemoteHost);
            else
                throw new Exception("Non-Remote client unable to send data to remote host");
        }

        public Task<UdpReceiveResult> StartWaitForData()
        {
            if (IsRemote)
            {
                var host = RemoteHost;
                return Sender.ReceiveAsync();
            }
            else
                throw new Exception("Non-Remote client unable to recieve data from remote host");
        }
        private Task<UdpReceiveResult> recieveTask;

        public void CheckForData()
        {
            if (recieveTask.IsCompletedSuccessfully)
            {
                HandleData(recieveTask.Result.Buffer);
            }
            if (recieveTask.IsCompletedSuccessfully || recieveTask.IsCompleted || recieveTask.IsFaulted || recieveTask.IsCanceled)
            {
                recieveTask = StartWaitForData();
            }
        }

        private bool[] ControlsMap = new bool[8];

        private void HandleData(byte[] bytes)
        {
            bool[] controlsMap = GetControlMap(bytes[0], Enum.GetValues<Control>().Count());
            for (byte i = 0; i < controlsMap.Length; i++)
            {
                ControlsMap[i] = controlsMap[i];
            }
        }

        private bool[] GetControlMap(byte data, int length)
        {
            bool[] boolArray = new bool[length];

            for (int i = 0; i < length; i++)
            {
                boolArray[i] = (data & (1 << i)) != 0;
            }

            return boolArray;
        }
        public void CreateRemoteControls()
        {
            Controls.ChangeControl(Control.left, () => ControlsMap[0]);
            Controls.ChangeControl(Control.right, () => ControlsMap[1]);
            Controls.ChangeControl(Control.jump, () => ControlsMap[2]);
            Controls.ChangeControl(Control.dash, () => ControlsMap[3]);
            Controls.ChangeControl(Control.pause, () => ControlsMap[4]);
        }
        #endregion
    }
}