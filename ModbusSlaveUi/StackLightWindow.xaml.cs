using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace ModbusSlaveUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StackLightWindow : Window
    {
        public MainWindow ModbusWindow { get; set; }
        public MyMainSlaveNetwork MainSlaveNetwork { get; set; }

        public ObservableCollection<string> SlColor { get; set; }
        public ObservableCollection<string> SlInputBit { get; set; }
        public ObservableCollection<string>  SlNbWord { get; set; }

        public string SlaveName { get; set; }
        public byte ByteId { get; set; }

        public CancellationTokenSource CancellationSourceToken { get; set; }

        public StackLightWindow(MainWindow modbusWindow, MyMainSlaveNetwork mainSlaveNetwork, string name, byte byteId, CancellationTokenSource cancellationTokenSource)
        {
            ModbusWindow = modbusWindow;
            MainSlaveNetwork = mainSlaveNetwork;
            InitializeComponent();
            Lb_Ip.Content += " " + ModbusWindow.Tb_Ip.Text;
            Lb_Port.Content += " " + ModbusWindow.Tb_Port.Text;

            SlColor = new ObservableCollection<string> { "Red", "Yellow", "Green" };
            LB_SlColor.ItemsSource = SlColor;

            SlInputBit = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", };
            LB_InputBit.ItemsSource = SlInputBit;

            SlNbWord = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5"};
            LB_NbWord.ItemsSource = SlNbWord;

            SlaveName = name;
            ByteId = byteId;

            Title = name;

            CancellationSourceToken = cancellationTokenSource;
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            MainSlaveNetwork.AddNewStacklightSlave(this, ByteId, SlaveName, CancellationSourceToken);
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            // here when the whole application is stopping, then I should cancel based on the token..?
            // also need to close the window
            this.Dispatcher.Invoke(delegate
            {
                CancellationSourceToken.Cancel();
                CancellationSourceToken.Dispose();
            });
        }

        // will be own by another thread??
        private void Btn_ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                this.Tbl_Log.Text = string.Empty;
            });
        }
    }
}
