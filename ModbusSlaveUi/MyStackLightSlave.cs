using ModbusSlaveUi;
using NModbus;
using SharedLibrary;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace StackLightSimulator
{
    public class MyStackLightSlave : Slave
    {
        public StackLightWindow StackLightWindow { get; set; }

        // need to have it change in the futur
        private bool _myLightState { get; set; } = true;
        private int _sLActive { get; set; } = 400;
        private int _sLInactive { get; set; } = 600;
        private int _sLWord { get; set; } = 1;
        private int _sLBit { get; set; } = 0;
        private int _sLRepetition { get; set; } = 1;
        private StackLightColor _sLColor { get; set; } = StackLightColor.None;

        // will be changed and added to the main class
        private CancellationTokenSource _cancellationTokenSource;

        public MyStackLightSlave(StackLightWindow stackLightWindow, IModbusFactory factory, IModbusSlaveNetwork slaveNetwork, string name, byte unitId, CancellationTokenSource cancellationTokenSource) : base(factory,slaveNetwork, name, unitId)
        {
            StackLightWindow = stackLightWindow;
            _cancellationTokenSource = cancellationTokenSource;
        }

        // need to check something based on the sender
        public override void StorageOperationOccuredForInputRegisterAction(object sender, StorageEventArgs<ushort> args)
        {
            WriteLog($"Input registers: {args.Operation} starting at {args.StartingAddress}");
        }

        public override void StorageOperationOccuredForHoldingRegisterAction(object sender, StorageEventArgs<ushort> args)
        {
            if (args.Points.Length == 3)
            {
                string sLWord = string.Join(" ", args.Points.Select(point => ((int)point).ToString()));
                WriteLog($"STACKLIGHT HEARD : Holding registers: {args.Operation} starting at {args.StartingAddress} with value : {sLWord}.");
                // HERE IS WANT TO START IT ONLY ONCE
                // NEED TO SET IT FOR ONLY ONCE AS WELL
                // How can I take control back on this thread
                var _mySlThread = new Thread(new ThreadStart(this.ListenToStackLightCall));
                _mySlThread.Start();
            }
        }

        public void ListenToStackLightCall()
        {
            var numberOfBlicking = 0;
            SetupStackLight();
            // HERE I BLOCK THE THREAD
            // I am blocking the thread of 
            while (_myLightState && numberOfBlicking < _sLRepetition && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // CAREFUL I AM GONNA MAKE THE MAIN THREAD SLEEP
                ChangeStackLightColor(_sLColor);
                Thread.Sleep(_sLActive);
                ChangeStackLightColor(StackLightColor.None);
                Thread.Sleep(_sLInactive);
                numberOfBlicking += 1;
            }
        }

        #region StackLightWindow
        public void SetupStackLight()
        {
            StackLightWindow.Dispatcher.Invoke(delegate
            {
                _sLActive = Int32.Parse(StackLightWindow.Tb_SeqActive.Text);
                _sLInactive = Int32.Parse(StackLightWindow.Tb_SeqInactive.Text);

                _sLRepetition = Int32.Parse(StackLightWindow.Tb_Repetition.Text);
                if (StackLightWindow.LB_NbWord.SelectedItem != null)
                    _sLWord = Int32.Parse(StackLightWindow.LB_NbWord.SelectedItem.ToString());
                else
                    _sLWord = 0;
                if (StackLightWindow.LB_InputBit.SelectedItem != null)
                    _sLBit = Int32.Parse(StackLightWindow.LB_InputBit.SelectedItem.ToString());
                else
                    _sLBit = 0;
                if (StackLightWindow.LB_SlColor.SelectedItem != null)
                    _sLColor = Enum.Parse<StackLightColor>(StackLightWindow.LB_SlColor.SelectedItem.ToString());
                else
                    _sLColor = StackLightColor.Red;
            });
        }

        public void ChangeStackLightColor(StackLightColor color)
        {
            StackLightWindow.Dispatcher.Invoke(delegate
            {
                switch (color)
                {
                    case StackLightColor.None:
                        StackLightWindow.RedLight.Fill = Brushes.Gray;
                        StackLightWindow.YellowLight.Fill = Brushes.Gray;
                        StackLightWindow.GreenLight.Fill = Brushes.Gray;
                        break;
                    case StackLightColor.Green:
                        StackLightWindow.RedLight.Fill = Brushes.Gray;
                        StackLightWindow.YellowLight.Fill = Brushes.Gray;
                        StackLightWindow.GreenLight.Fill = Brushes.Green;
                        break;
                    case StackLightColor.Yellow:
                        StackLightWindow.RedLight.Fill = Brushes.Gray;
                        StackLightWindow.YellowLight.Fill = Brushes.Yellow;
                        StackLightWindow.GreenLight.Fill = Brushes.Gray;
                        break;
                    case StackLightColor.Red:
                        StackLightWindow.RedLight.Fill = Brushes.Red;
                        StackLightWindow.YellowLight.Fill = Brushes.Gray;
                        StackLightWindow.GreenLight.Fill = Brushes.Gray;
                        break;
                }
            });
        }

        public void WriteLog(string log)
        {
            StackLightWindow.Dispatcher.Invoke(delegate
            {
                StackLightWindow.Tbl_Log.Text += "\n" + log;
            });
        }
        #endregion
    }
}
