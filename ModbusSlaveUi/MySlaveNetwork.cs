using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModbusSlaveUi
{
    public class MySlaveNetwork
    {
        private int Port { get; set; }
        private IPAddress IPAddress { get; set; }
        private TcpListener SlaveTcpListener { get; set; }
        public MainWindow UiWindow { get; set; }

        private List<string> SelectedOutput { get; set; } = new();
        private int SelectedInputWords { get; set; }
        private IModbusSlave _mySlave {get; set;}
        private ushort[] _outputWords { get; set; } = new ushort[] { };


        #region SL
        private bool _myLightState { get; set; } = false;
        private int _sLActive { get; set; } = 400;
        private int _sLInactive { get; set; } = 600;
        private int _sLWord { get; set; } = 1;
        private int _sLBit { get; set; } = 0;
        private int _sLRepetition { get; set; } = 1;
        private StackLightColor _sLColor { get; set; } = StackLightColor.None;
        #endregion
        public MySlaveNetwork(MainWindow uiWindow, IPAddress ip, int port) 
        {
            this.IPAddress= ip;
            this.Port= port;
            this.SlaveTcpListener = new TcpListener(IPAddress, Port);
            UiWindow = uiWindow;
        } 

        public void OnStart()
        {
            SlaveTcpListener.Start();
            ChangeStackLightColor(StackLightColor.None);
            IModbusFactory factory = new ModbusFactory();
            IModbusSlaveNetwork network = CreateMySlaveNetwork(SlaveTcpListener, factory);
            network.ListenAsync().GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }

        public void OnStop()
        {
            SlaveTcpListener.Stop();
        }

        public IModbusSlaveNetwork CreateMySlaveNetwork(TcpListener slaveTcpListener, IModbusFactory factory)
        {
            IModbusSlaveNetwork network = factory.CreateSlaveNetwork(slaveTcpListener);
            _mySlave = CreateMySlave(0, factory);
            network.AddSlave(_mySlave);
            WriteLog("tcp Listener started, and slaveNetwork created");
            return network;
        }

        public IModbusSlave CreateMySlave(byte byteId, IModbusFactory factory)
        {
            var slaveDataStore = new SlaveStorage();
            slaveDataStore.InputRegisters.StorageOperationOccurred += (sender, args) => StorageOperationOccuredForInputRegisterAction(sender, args);
            slaveDataStore.HoldingRegisters.StorageOperationOccurred += (sender, args) => StorageOperationOccuredForHoldingRegisterAction(sender, args);
            return factory.CreateSlave(byteId, slaveDataStore);
        }

        // need to check something based on the sender
        public void StorageOperationOccuredForInputRegisterAction(object sender, StorageEventArgs<ushort> args)
        {
            // here I should change based on the bytes being sent
            WriteLog($"Input registers: {args.Operation} starting at {args.StartingAddress}");
            ChangeStackLightColor(StackLightColor.Yellow);
        }

        // need to check something based on the sender
        public void StorageOperationOccuredForHoldingRegisterAction(object sender, StorageEventArgs<ushort> args)
        {
            if(args.Points.Length == SelectedInputWords)
            {
                string pointWord = string.Join(" ", args.Points.Select(point => ((int)point).ToString()));
                WriteLog($"Holding registers: {args.Operation} starting at {args.StartingAddress} with value : {pointWord}.");
                ChangeStackLightColor(StackLightColor.Green);
            }
            if (_myLightState && _sLWord < args.Points.Length)
            {
                if (args.Points[_sLWord] == _sLBit)
                {
                    string sLWord = string.Join(" ", args.Points.Select(point => ((int)point).ToString()));
                    WriteLog($"STACKLIGHT HEARD : Holding registers: {args.Operation} starting at {args.StartingAddress} with value : {sLWord}.");
                    ListenToStackLightCall();
                }
            }
        }

        public void WriteInHoldingRegisterFromSlave()
        {
            ushort startAdress = ushort.Parse(UiWindow.Tb_StartAdress.Text);
            _mySlave.DataStore.HoldingRegisters.WritePoints(startAdress, _outputWords);
            _outputWords = new ushort[]{ };
            ClearOutputWord();
        }

        public void SLOnStart()
        {
            // HERE ADD ALL THE REFERENCES
            WriteLog($"Start listening for StackLight bit {_sLBit} and word : {_sLWord}");
            SetupStackLight();
            _myLightState = true;
        }
        public void SetupStackLight()
        {
            UiWindow.Dispatcher.Invoke(delegate
            {
                _sLBit = Int32.Parse(UiWindow.Tb_SLAssignedBit.Text);
                _sLActive = Int32.Parse(UiWindow.Tb_SLActive.Text);
                _sLInactive = Int32.Parse(UiWindow.Tb_SLIntactive.Text);

                _sLRepetition = Int32.Parse(UiWindow.Tb_SLRepetion.Text);
                if (UiWindow.myListBoxStackLightInputBits.SelectedItem != null)
                    _sLWord = Int32.Parse(UiWindow.myListBoxStackLightInputBits.SelectedItem.ToString());
                else
                    _sLWord = 1;
                if (UiWindow.myListBoxStackLightColor.SelectedItem != null)
                    _sLColor = Enum.Parse<StackLightColor>(UiWindow.myListBoxStackLightColor.SelectedItem.ToString());
                else
                    _sLColor = StackLightColor.Red;
            });
        }
        public void ListenToStackLightCall()
        {
            var numberOfBlicking = 0;
            while (_myLightState && numberOfBlicking < _sLRepetition)
            {
                // CAREFUL I AM GONNA MAKE THE MAIN THREAD SLEEP
                ChangeStackLightColor(_sLColor);
                Thread.Sleep(_sLActive);
                ChangeStackLightColor(StackLightColor.None);
                Thread.Sleep(_sLInactive);
                numberOfBlicking += 1;
            }
        }

        #region Helper Graphic
        public void WriteLog(string log)
        {
            UiWindow.Dispatcher.Invoke(delegate
            {
                UiWindow.Tbl_Infos.Text += "\n" + log;
            });
        }

        public void CLearLog()
        {
            UiWindow.Dispatcher.Invoke(delegate
            {
                UiWindow.Tbl_Infos.Text = string.Empty;
            });
        }


        public void AddOutputWordToTbl(string word)
        {
            UiWindow.Dispatcher.Invoke(delegate
            {
                UiWindow.Tbl_OutputBits.Text += " " + word;
            });
        }

        public void ClearOutputWord()
        {
            UiWindow.Dispatcher.Invoke(delegate
            {
                UiWindow.Tbl_OutputBits.Text = "";
            });
        }

        public void ChangeStackLightColor(StackLightColor color)
        {
            UiWindow.Dispatcher.Invoke(delegate
            {
                switch (color)
                {
                    case StackLightColor.None:
                        UiWindow.RedLight.Fill = Brushes.Gray;
                        UiWindow.YellowLight.Fill = Brushes.Gray;
                        UiWindow.GreenLight.Fill = Brushes.Gray;
                        break;
                    case StackLightColor.Green:
                        UiWindow.RedLight.Fill = Brushes.Gray;
                        UiWindow.YellowLight.Fill = Brushes.Gray;
                        UiWindow.GreenLight.Fill = Brushes.Green;
                        break;
                    case StackLightColor.Yellow:
                        UiWindow.RedLight.Fill = Brushes.Gray;
                        UiWindow.YellowLight.Fill = Brushes.Yellow;
                        UiWindow.GreenLight.Fill = Brushes.Gray;
                        break;
                    case StackLightColor.Red:
                        UiWindow.RedLight.Fill = Brushes.Red;
                        UiWindow.YellowLight.Fill = Brushes.Gray;
                        UiWindow.GreenLight.Fill = Brushes.Gray;
                        break;
                }
            });
        }

        public void InputBitSelectionChanged(string inputBitWords)
        {
            SelectedInputWords = Int32.Parse(inputBitWords);
        }

        public void OutputBitSelectionChanged(string outputBit)
        {
            if (SelectedOutput.Contains(outputBit))
                SelectedOutput.Remove(outputBit);
            else
                SelectedOutput.Add(outputBit);
        }

        public void AddToOutputBits()
        {
            string myUshortString = string.Empty;
            int myUshortInt = 0;
            for (int i = 0; i < UiWindow.myListBoxOutputBits.Items.Count; i++)
            {
                if (SelectedOutput.Contains(UiWindow.myListBoxOutputBits.Items[i]))
                {
                    myUshortString = "1" + myUshortString;
                    myUshortInt += (int)Math.Pow(2, i);
                }
                else
                    myUshortString = "0" + myUshortString;
            }
            AddOutputWordToTbl(myUshortInt.ToString());
            var arrayResized = _outputWords;
            Array.Resize(ref arrayResized, arrayResized.Length + 1);
            arrayResized[arrayResized.Length - 1] = (ushort)myUshortInt;
            _outputWords = arrayResized;

            // unselect the output bits
            UiWindow.myListBoxOutputBits.UnselectAll();
        }
        #endregion
    }

    public enum StackLightColor
    {
        None = 0,
        Green,
        Yellow,
        Red
    }
}
