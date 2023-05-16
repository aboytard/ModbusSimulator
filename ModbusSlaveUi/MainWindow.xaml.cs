using NModbus;
using NModbus.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace ModbusSlaveUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MySlaveNetwork _mySlaveNetwork;
        private MySlaveNetwork _mySLSlaveNetwork;

        public ObservableCollection<string> MyInputBits { get; set; }
        public ObservableCollection<string> MyOutputBits { get; set; }
        public ObservableCollection<string> MyStackLightInputBits { get; set; }
        public ObservableCollection<string> MyStackLightColors { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            MyInputBits = new ObservableCollection<string> { "0", "1", "2", "3", "4","5"};
            myListBoxInputBits.ItemsSource = MyInputBits;

            // TO CHANGE
            // here I should give the possibility to chose only based on the MyInputBits selected value
            MyStackLightInputBits = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", };
            myListBoxStackLightInputBits.ItemsSource = MyInputBits;

            MyStackLightInputBits = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", };

            MyStackLightColors = new ObservableCollection<string> { "Red", "Yellow", "Green" };
            myListBoxStackLightColor.ItemsSource = MyStackLightColors;

            MyOutputBits = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "10", "11", "12", "13", "14", "15" };
            myListBoxOutputBits.ItemsSource = MyOutputBits;
        }

        private void Btn_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            _mySlaveNetwork.WriteInHoldingRegisterFromSlave();
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mySlaveNetwork = new MySlaveNetwork(this, IPAddress.Parse(Tb_Ip.Text), Int32.Parse(Tb_Port.Text));
                var _myServerThread = new Thread(new ThreadStart(_mySlaveNetwork.OnStart));
                _myServerThread.Start();
            }
            catch (Exception exeption)
            {
                Console.WriteLine(exeption.Message);
            }
        }

        private void myListBoxInputBits_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                _mySlaveNetwork.InputBitSelectionChanged(e.AddedItems[0].ToString());
        }

        private void myListBoxOutputBits_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                _mySlaveNetwork.OutputBitSelectionChanged(e.AddedItems[0].ToString());
            if (e.RemovedItems.Count > 0)
                _mySlaveNetwork.OutputBitSelectionChanged(e.RemovedItems[0].ToString());
        }

        private void Btn_ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            _mySlaveNetwork.CLearLog();
        }

        private void Btn_AddOutputBit_Click(object sender, RoutedEventArgs e)
        {
            _mySlaveNetwork.AddToOutputBits();
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            _mySlaveNetwork.OnStop();
        }

        private void Btn_StackLight_Click(object sender, RoutedEventArgs e)
        {
            //_mySLSlaveNetwork = new MySlaveNetwork(this, IPAddress.Parse(Tb_Ip.Text), Int32.Parse(Tb_Port.Text));
            //var _mySLThread = new Thread(new ThreadStart(_mySLSlaveNetwork.SLOnStart));
            //_mySLThread.Start();

            _mySlaveNetwork.SLOnStart();
        }
    }
}
