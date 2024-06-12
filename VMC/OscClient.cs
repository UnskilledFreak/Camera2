using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Camera2.Behaviours;
using JetBrains.Annotations;

namespace Camera2.VMC
{
    internal class OscClient : IDisposable
    {
        private readonly Socket _socket;
        private bool _disposed;

        public OscClient(IPEndPoint destination)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            if (destination.Address.ToString() == "255.255.255.255")
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            }

            _socket.Connect(destination);
        }

        ~OscClient() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* Hardcoded for performance and simplicity because I probably will never need to send
         * any other commands / differently formatted ones
         */
        private static readonly byte[] Buffer = Encoding.ASCII.GetBytes(
            "/VMC/Ext/Cam\0\0\0\0" +
            ",sffffffff\0\0" +
            "Camera\0\0" +
            "XPOSYPOSZPOSXROTYROTZROTWROT_FOV"
        );

        public unsafe void SendCamPos(Cam2 camera)
        {
            fixed (byte* prt = Buffer)
            {
                uint Rev(float x)
                {
                    var value = ( byte* )&x;

                    // todo :: check if inline increment parenthesis are needed
                    return ( uint )(*(value++) << 24) | ( uint )(*(value++) << 16) | ( uint )(*(value++) << 8) | *value;
                }

                var p = ( uint* )(prt + Buffer.Length - (4 * 8));

                p[0] = Rev(camera.TransformChain.Position.x);
                p[1] = Rev(camera.TransformChain.Position.y);
                p[2] = Rev(camera.TransformChain.Position.z);
                p[3] = Rev(camera.TransformChain.Rotation.x);
                p[4] = Rev(camera.TransformChain.Rotation.y);
                p[5] = Rev(camera.TransformChain.Rotation.z);
                p[6] = Rev(camera.TransformChain.Rotation.w);
                p[7] = Rev(camera.Settings.FOV);
            }

            _socket.Send(Buffer);
        }

        [UsedImplicitly]
        public void Dispose(bool disposing)
        {
            if (!_disposed && (_disposed = true) && disposing)
            {
                _socket?.Close();
            }
        }
    }
}