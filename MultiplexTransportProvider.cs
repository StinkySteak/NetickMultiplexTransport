using Netick.Unity;
using System.Reflection;
using UnityEngine;

namespace Netick.Transport
{
    [CreateAssetMenu(fileName = "MultiplexTransportProvider", menuName = "Netick/Transport/MultiplexTransportProvider", order = 1)]
    public class MultiplexTransportProvider : NetworkTransportProvider
    {
        [SerializeField] private NetworkTransportProvider[] _transports;
        [SerializeField] private int _portOffsetPerTransport = 10;
        public int ClientTransportIndex;

        public override NetworkTransport MakeTransportInstance()
        {
            MultiplexTransport multiplex = new MultiplexTransport();
            multiplex._portOffsetPerTransport = _portOffsetPerTransport;
            multiplex._transports = new NetworkTransport[_transports.Length];
            multiplex._clientTransportIndex = ClientTransportIndex;

            for (int i = 0; i < _transports.Length; i++)
            {
                NetworkTransportProvider constructor = _transports[i];

                NetworkTransport networkTransport = constructor.MakeTransportInstance();

                multiplex._transports[i] = networkTransport;
            }

            return multiplex;
        }

        internal class MultiplexTransport : NetworkTransport
        {
            internal NetworkTransport[] _transports;
            internal int _portOffsetPerTransport = 10;
            internal int _clientTransportIndex;

            public override void ForceUpdate()
            {
                foreach (NetworkTransport transport in _transports)
                    transport.ForceUpdate();
            }

            public override void Init()
            {
                object[] parameters = new object[] { NetworkPeer, Engine };

                foreach (NetworkTransport transport in _transports)
                {
                    // TODO: Replace reflection when internal init has been exposed
                    MethodInfo internalInit = transport.GetType().GetMethod("_Init", BindingFlags.NonPublic | BindingFlags.Instance);

                    internalInit.Invoke(transport, parameters);
                    transport.Init();
                }
            }

            public override void Connect(string address, int port, byte[] connectionData, int connectionDataLength)
            {
                foreach (NetworkTransport transport in _transports)
                    transport.Connect(address, port, connectionData, connectionDataLength);
            }

            public override void Disconnect(TransportConnection connection)
            {
                foreach (NetworkTransport transport in _transports)
                    transport.Disconnect(connection);
            }

            public override void PollEvents()
            {
                foreach (NetworkTransport transport in _transports)
                    transport.PollEvents();
            }

            public override void Run(RunMode mode, int port)
            {
                if (mode == RunMode.Client)
                {
                    NetworkTransport transport = _transports[_clientTransportIndex];

                    transport.Run(mode, port);
                    return;
                }


                int nextPort = port;
                foreach (NetworkTransport transport in _transports)
                {
                    transport.Run(mode, nextPort);
                    Debug.Log($"[{nameof(MultiplexTransport)}] Running {transport} on {nextPort}");

                    nextPort += _portOffsetPerTransport;
                }
            }

            public override void Shutdown()
            {
                foreach (NetworkTransport transport in _transports)
                    transport.Shutdown();
            }
        }
    }
}
