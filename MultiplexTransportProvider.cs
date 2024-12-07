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
                }
            }

            public override void Connect(string address, int port, byte[] connectionData, int connectionDataLength)
            {
                if (TryGetClientTransport(out NetworkTransport transport))
                {
                    int clientPortOffset = _portOffsetPerTransport * _clientTransportIndex;
                    int clientPort = port + clientPortOffset;

                    Debug.Log($"[{nameof(MultiplexTransport)}]: Connecting client using {transport} to: {clientPort}");
                    transport.Connect(address, clientPort, connectionData, connectionDataLength);
                }
            }

            private bool TryGetClientTransport(out NetworkTransport networkTransport)
            {
                bool isOutOfBounds = _clientTransportIndex < 0 || _clientTransportIndex >= _transports.Length;

                if (isOutOfBounds)
                {
                    Debug.LogError($"[{nameof(MultiplexTransport)}] Failed to get client transport, index is out of bounds");
                    networkTransport = null;
                    return false;
                }

                NetworkTransport transport = _transports[_clientTransportIndex];
                networkTransport = transport;
                return true;
            }

            public override void Disconnect(TransportConnection connection)
            {
                if (Engine.IsClient)
                {
                    if (TryGetClientTransport(out NetworkTransport transport))
                        transport.Disconnect(connection);

                    return;
                }

                foreach (NetworkTransport transport in _transports)
                {
                    //Debug.Log($"[{nameof(MultiplexTransport)}]: {transport} Disconnecting: {connection}");
                    bool hasException = false;

                    try
                    {
                        transport.Disconnect(connection);
                    }
                    catch (System.Exception)
                    {
                        hasException = true;
                        Debug.LogWarning($"[{nameof(MultiplexTransport)}]: Disconnection failed: trying to disconnect: {connection} from: {transport}");
                    }

                    if (!hasException)
                    {
                        break;
                    }
                }
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
                    RunClient(port);
                    return;
                }

                RunServer(port);
            }

            private void RunClient(int port)
            {
                if (TryGetClientTransport(out NetworkTransport transport))
                    transport.Run(RunMode.Client, port);
            }

            private void RunServer(int port)
            {
                int serverPort = port;

                foreach (NetworkTransport transport in _transports)
                {
                    Debug.Log($"[{nameof(MultiplexTransport)}]: Running server {transport} on {serverPort}");
                    transport.Run(RunMode.Server, serverPort);

                    serverPort += _portOffsetPerTransport;
                }
            }

            public override void Shutdown()
            {
                foreach (NetworkTransport transport in _transports)
                {
                    Debug.Log($"[{nameof(MultiplexTransport)}]: shutdown: {transport} isServer: {Engine.IsServer}");
                    transport.Shutdown();
                }
            }
        }
    }
}
