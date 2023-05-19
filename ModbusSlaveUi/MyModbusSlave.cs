using NModbus;
using SharedLibrary;
using System.Linq;

namespace ModbusSlaveUi
{
    public class MyModbusSlave : Slave
    {
        public MainWindow ModbusSlaveWindow { get; set; }
        private string pointWord { get; set; }

        public MyModbusSlave(MainWindow window, IModbusFactory networkFactory, IModbusSlaveNetwork slaveNetwork, string name, byte unitId) : base(networkFactory, slaveNetwork, name, unitId)
        {
            ModbusSlaveWindow = window;
        }

        // need to check something based on the sender
        public override void StorageOperationOccuredForInputRegisterAction(object sender, StorageEventArgs<ushort> args)
        {
            // HERE NEED TO PUT
            WriteLog($"Input registers: {args.Operation} starting at {args.StartingAddress}");
        }

        public override void StorageOperationOccuredForHoldingRegisterAction(object sender, StorageEventArgs<ushort> args)
        {
            //HERE STILL NEED TO HAVE THE NUMBER OF WORD CHECKED
            if (args.Points.Length == ModbusSlaveWindow.SelectedNumberOfInputBit)
            {
                var newPointWord = string.Join(" ", args.Points.Select(point => ((int)point).ToString()));
                if (pointWord != newPointWord || string.IsNullOrEmpty(pointWord))
                {
                    WriteLog($"Holding registers: {args.Operation} starting at {args.StartingAddress} with value : {newPointWord}.");
                    pointWord = newPointWord;
                }
            }
        }

        public void WriteLog(string log)
        {
            ModbusSlaveWindow.Dispatcher.Invoke(delegate
            {
                ModbusSlaveWindow.Tbl_Infos.Text += "\n" + log;
            });
        }
    }
}
