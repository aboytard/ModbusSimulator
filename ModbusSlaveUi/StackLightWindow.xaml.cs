using SharedLibrary;
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
        public StackLight StackLight { get; set; }
        public StackLightWindow(MainWindow modbusWindow, MyMainSlaveNetwork mainSlaveNetwork, StackLight stackLight)
        {
            ModbusWindow = modbusWindow;
            MainSlaveNetwork = mainSlaveNetwork;
            InitializeComponent();
            StackLight = stackLight;
            Title = stackLight.Name;
            this.Dispatcher.Invoke(delegate
            {
                this.Tbl_Log.Text = $"SL {StackLight.Name} {StackLight.ByteId}  with value {StackLight.InputBit} words: {StackLight.NbWord}";
            });
        }

        private void Btn_ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                this.Tbl_Log.Text = string.Empty;
            });
        }
    }
}
