using SharedLibrary;
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
        private MyMainSlaveNetwork MyMainSlaveNetwork;

        public ObservableCollection<string> MyInputBits { get; set; }
        public ObservableCollection<string> MyOutputBits { get; set; }

        // key : name of the slave, value : byteId
        public Dictionary<string, byte> _slaveMapping = new();
        public ObservableCollection<string> _slaves { get; set; } = new();

        // key: name of the slave / Title of the window, value : cancellation token in of the slave
        public Dictionary<string, CancellationTokenSource> _runningSlaveCancellationToken = new();
        public Dictionary<string, Window> _runningWindows = new();

        public Thread MyListenerThread { get; set; }

        #region StackLight
        public StackLight currentStackLight { get; set; }
        public ObservableCollection<string> SlColor { get; set; }
        public ObservableCollection<string> SlInputBit { get; set; }
        public ObservableCollection<string> SlNbWord { get; set; }
        #endregion
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

            SlColor = new ObservableCollection<string> { "Red", "Yellow", "Green" };
            LB_SlColor.ItemsSource = SlColor;

            SlInputBit = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", };
            LB_InputBit_Sl.ItemsSource = SlInputBit;

            SlNbWord = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5" };
            LB_NbWord.ItemsSource = SlNbWord;
        }

        private void Btn_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            foreach(var slave in SelectedIODeviceToSend)
            {
                MyMainSlaveNetwork.mySlaveMapping[slave].WriteInHoldingRegisterFromSlave(_startAdress, _outputWords);
            }
            _outputWords = new ushort[] { };
            ClearOutputWord();
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _slaves.Add("ModbusSlave");
                _slaveMapping.TryAdd("ModbusSlave", (byte)Int32.Parse("0"));
                MyMainSlaveNetwork = new MyMainSlaveNetwork(this, IPAddress.Parse(Tb_Ip.Text), Int32.Parse(Tb_Port.Text));
                MyListenerThread = new Thread(new ThreadStart(MyMainSlaveNetwork.OnTcpConnectionChangeState));
                MyListenerThread.Start();
            }
            catch (Exception exeption)
            {
                Console.WriteLine(exeption.Message);
            }
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            MyListenerThread.Join(TimeSpan.FromSeconds(1));
            MyMainSlaveNetwork.OnStop();
            foreach(var pair in _runningSlaveCancellationToken)
            {
                pair.Value.Cancel();
            }
            foreach(var pair in _runningWindows)
            {
                pair.Value.Close();
            }
            // Can create a Dispose method
            _runningWindows.Clear();
            _runningSlaveCancellationToken.Clear();
            MyMainSlaveNetwork.mySlaveMapping.Clear();
            _slaveMapping.Clear();
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

        #region Ui-Helper
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

        public void SetupStackLight()
        {
            Dispatcher.Invoke(delegate
            {
                currentStackLight = new StackLight()
                {
                    Ip = Tb_Ip.Text,
                    Port = Int32.Parse(Tb_Port.Text.ToString()),
                    Name = Tb_Name_Sl.Text,
                    ByteId = (byte)Int32.Parse(Tb_ByteId_Sl.Text),
                    Active = Int32.Parse(Tb_SeqActive.Text),
                    Inactive = Int32.Parse(Tb_SeqInactive.Text),
                    Repetition = Int32.Parse(Tb_Repetition.Text),
                    NbWord = Int32.Parse(LB_NbWord.SelectedItem.ToString()),
                    InputBit = Int32.Parse(LB_InputBit_Sl.SelectedItem.ToString()),
                    Color = Enum.Parse<StackLightColor>(LB_SlColor.SelectedItem.ToString())
                };
            });
        }

        private void Btn_ClearMessage_Click(object sender, RoutedEventArgs e)
        {
            _outputWords = new ushort[] { };
            ClearOutputWord();
        }

        private void Btn_ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                Tbl_Infos.Text = "";
            });
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




        private void Btn_CreateSl_Click(object sender, RoutedEventArgs e)
        {
            // HERE I WILL CREATE THE MAPPING BASED ON THE CANCELLATIONTOKEN
            var cancellationSourceToken = new CancellationTokenSource();
            Dispatcher.Invoke(delegate
            {
                SetupStackLight();
                var stackLightWindow = new StackLightWindow(this, MyMainSlaveNetwork, currentStackLight);
                stackLightWindow.Show();
                var slaveByteId = (byte)(Int32.Parse(Tb_ByteId_Sl.Text));
                MyMainSlaveNetwork.AddNewStacklightSlave(stackLightWindow, stackLightWindow.StackLight, cancellationSourceToken);

                // mapping
                _slaveMapping.TryAdd(Tb_Name_Sl.Text, slaveByteId);
                _slaves.Add(Tb_Name_Sl.Text);
                _runningWindows.TryAdd(Tb_Name_Sl.Text, stackLightWindow);
                _runningSlaveCancellationToken.TryAdd(Tb_Name_Sl.Text, cancellationSourceToken);
            });
        }

    }
}
