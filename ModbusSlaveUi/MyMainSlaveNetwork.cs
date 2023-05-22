using SharedLibrary;
using StackLightSimulator;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace ModbusSlaveUi
{
    public class MyMainSlaveNetwork : SlaveNetwork
    {
        public MainWindow UiWindow { get; set; }

        public MyMainSlaveNetwork(MainWindow uiWindow, IPAddress ip, int port) : base(ip, port)
        {
            UiWindow = uiWindow;
        } 

        public override void OnStart()
        {
            try
            {
                if (IsSlaveTcpListenerOpen)
                {
                    base.CreateMySlaveNetwork(SlaveTcpListener);
                    WriteLog("Listener server start");
                    SlaveTcpListener.Start();
                    CreateMySlaveNetwork(SlaveTcpListener);
                    Network.ListenAsync(NetworkToken.Token).GetAwaiter().GetResult();
                }
            }
            catch
            {
                UiWindow.Dispatcher.Invoke(delegate
                {
                    UiWindow.Tbl_Infos.Text += "\n" + "Network thread stopped listening." + "\n" + "TcpListener stopped listening.";
                });
            }
        }

        public override void CreateMySlaveNetwork(TcpListener slaveTcpListener)
        {
            base.CreateMySlaveNetwork(slaveTcpListener);
            // this is my default slave in the network
            AddNewSlave(0, "ModbusSlave");
        }

        public override void AddSlaveToNetwork(Slave slave)
        {
            base.AddSlaveToNetwork(slave);
        }


        // USE ABSTRACT TO HAVE THE METHOD BEING USED BY BOTH IN THE SAME ONE???
        public override void AddNewSlave(byte byteId, string name)
        {
            var slaveToAdd = new MyModbusSlave(UiWindow, _networkFactory, Network, name, byteId);
            AddSlaveToNetwork(slaveToAdd);
            WriteLog($"New slave added: {name} {byteId}");
        }

        public void AddNewStacklightSlave(StackLightWindow stackLightWindow,byte byteId, string name, CancellationTokenSource cancellationTokenSource)
        {
            // ADD CANCELLATION TOKEN?
            var slaveToAdd = new MyStackLightSlave(stackLightWindow, _networkFactory, Network, name, byteId, cancellationTokenSource);
            AddSlaveToNetwork(slaveToAdd);
            WriteLog($"New slave added: {name} {byteId}");
        }

        #region Helper Graphic
        public void WriteLog(string log)
        {
            UiWindow.Dispatcher.Invoke(delegate
            {
                UiWindow.Tbl_Infos.Text += "\n" + log;
            });
        }
        #endregion
    }
}
