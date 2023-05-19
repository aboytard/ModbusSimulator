using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ModbusSlaveUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyMainSlaveNetwork _myTcpListener;

        public ObservableCollection<string> MyInputBits { get; set; }
        public ObservableCollection<string> MyOutputBits { get; set; }

        // key : name of the slave, value : byteId
        public Dictionary<string, byte> _slaveMapping = new();
        public ObservableCollection<string> _slaves { get; set; } = new();

        #region Input
        public int SelectedNumberOfInputBit { get; set; }
        #endregion

        #region Output
        private ushort[] _outputWords { get; set; } = new ushort[] { };
        private List<string> _selectedOutputBit = new();
        private ushort _startAdress;
        private List<string> SelectedIODeviceToSend = new();
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            MyInputBits = new ObservableCollection<string> { "0", "1", "2", "3", "4","5"};
            myListBoxInputBits.ItemsSource = MyInputBits;

            MyOutputBits = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "10", "11", "12", "13", "14", "15" };
            myListBoxOutputBits.ItemsSource = MyOutputBits;

            myListSlaves.ItemsSource = _slaves;
            _slaves.Add("ModbusSlave");
            _slaveMapping.TryAdd("ModbusSlave", (byte)Int32.Parse("0"));
        }

        private void Btn_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            foreach(var slave in SelectedIODeviceToSend)
            {
                _myTcpListener.mySlaveMapping[slave].WriteInHoldingRegisterFromSlave(_startAdress, _outputWords);
            }
            //_myTcpListener.mySlaveMapping["ModbusSlave"].WriteInHoldingRegisterFromSlave(_startAdress, _outputWords);

            //// if I want to write in the stackLightRegister
            //_myTcpListener.mySlaveMapping["StackLightSlave"].WriteInHoldingRegisterFromSlave(_startAdress, _outputWords);

            _outputWords = new ushort[] { };
            ClearOutputWord();
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _myTcpListener = new MyMainSlaveNetwork(this, IPAddress.Parse(Tb_Ip.Text), Int32.Parse(Tb_Port.Text));
                var _myListenerThread = new Thread(new ThreadStart(_myTcpListener.OnTcpConnectionChangeState));
                _myListenerThread.Start();
            }
            catch (Exception exeption)
            {
                Console.WriteLine(exeption.Message);
            }
        }

        private void myListBoxInputBits_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                SelectedNumberOfInputBit = Int32.Parse(e.AddedItems[0].ToString());
        }

        private void myListSlaves_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                SelectedIODeviceToSend.Add(e.AddedItems[0].ToString());
            if (e.RemovedItems.Count > 0)
                SelectedIODeviceToSend.Remove(e.RemovedItems[0].ToString());
        }

        private void myListBoxOutputBits_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                OutputBitSelectionChanged(e.AddedItems[0].ToString());
            if (e.RemovedItems.Count > 0)
                OutputBitSelectionChanged(e.RemovedItems[0].ToString());
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            _myTcpListener.OnStop();
        }

        private void Btn_TestOpen_Click(object sender, RoutedEventArgs e)
        {
            AddNewDevice();
        }

        #region Ui-Helper

        public void AddNewDevice()
        {
            Dispatcher.Invoke(delegate 
            {
                var stackLightWindow = new StackLightWindow(this, _myTcpListener,Tb_Name.Text, (byte)Int32.Parse(Tb_ByteId.Text));
                stackLightWindow.Show();
                _slaveMapping.TryAdd(Tb_Name.Text, (byte)(Int32.Parse(Tb_ByteId.Text)));
                _slaves.Add(Tb_Name.Text);
            });
        }
        public void AddOutputWordToTbl(string word)
        {
            Dispatcher.Invoke(delegate
            {
                Tbl_OutputBits.Text += " " + word;
            });
        }

        public void ClearOutputWord()
        {
            Dispatcher.Invoke(delegate
            {
                Tbl_OutputBits.Text = "";
            });
        }

        public void OutputBitSelectionChanged(string outputBit)
        {
            if (_selectedOutputBit.Contains(outputBit))
                _selectedOutputBit.Remove(outputBit);
            else
                _selectedOutputBit.Add(outputBit);
        }
        #endregion

        private void Btn_AddOutputBit_Click(object sender, RoutedEventArgs e)
        {
            string myUshortString = string.Empty;
            int myUshortInt = 0;
            for (int i = 0; i < myListBoxOutputBits.Items.Count; i++)
            {
                if (_selectedOutputBit.Contains(this.myListBoxOutputBits.Items[i].ToString()))
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
            _startAdress = (ushort)(Int32.Parse(Tb_StartAdress.Text));

            // unselect the output bits
            myListBoxOutputBits.UnselectAll();
            _selectedOutputBit.Clear();
        }

        private void Btn_ClearMessage_Click(object sender, RoutedEventArgs e)
        {
            _outputWords = new ushort[] { };
            ClearOutputWord();        }

        private void Btn_ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                Tbl_Infos.Text = "";
            });
        }
    }
}
