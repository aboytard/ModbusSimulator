using NModbus;
using System.Net;
using System.Net.Sockets;

namespace SharedLibrary
{
    public class SlaveNetwork
    {
        // define the tcpConnection
        public int Port { get; set; }
        public IPAddress IPAddress { get; set; }
        public TcpListener SlaveTcpListener { get; set; }

        // define the networkState
        public bool IsSlaveTcpListenerOpen { get; set; } = false;

        // key: name of the slave, value: slave with its own handler method
        public IDictionary<string, Slave> mySlaveMapping { get; set; } = new Dictionary<string,Slave>();

        public IModbusSlaveNetwork Network { get; set; }
        public IModbusFactory _networkFactory { get; set; } = new ModbusFactory();

        public SlaveNetwork(IPAddress ip, int port)
        {
            IPAddress = ip;
            Port = port;
            SlaveTcpListener = new TcpListener(IPAddress, Port);
            _networkFactory = new ModbusFactory();
        }

        public void OnTcpConnectionChangeState()
        {
            IsSlaveTcpListenerOpen = !IsSlaveTcpListenerOpen;
            if (IsSlaveTcpListenerOpen)
            {
                OnStart();
                CreateMySlaveNetwork(SlaveTcpListener);
            }
            else
            {
                // delete all --> 
                // delete all slave
                // put back every byte in the storage from the master to 0
                // OnStop();
                SlaveTcpListener.Stop();
            }
        }


        public virtual void OnStart()
        {
            SlaveTcpListener.Start();
            CreateMySlaveNetwork(SlaveTcpListener);
            Network.ListenAsync().GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }

        public void OnStop()
        {
            throw new NotImplementedException();
        }

        public virtual void CreateMySlaveNetwork(TcpListener slaveTcpListener)
        {
            Network = _networkFactory.CreateSlaveNetwork(slaveTcpListener);
        }

        public virtual void AddSlaveToNetwork(Slave slave)
        {
            Network.AddSlave(slave._slave);
            mySlaveMapping.TryAdd(slave.Name, slave);
        }

        // HERE I SHOULD ADD TWO DELEGATE THAT CAN THEN BE CHANGED DEPENDING
        public virtual void AddNewSlave(byte byteId, string name)
        {
            var slaveToAdd = new Slave(_networkFactory, Network,name, byteId);
            AddSlaveToNetwork(slaveToAdd);
        }
    }
}
