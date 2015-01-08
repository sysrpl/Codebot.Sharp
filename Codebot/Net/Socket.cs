using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Codebot.Net
{

	public static class Network
	{
		public static Socket CreateSocket()
		{
			return new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
		}

		public static EndPoint CreateEndPoint(string hostName, int port)
		{
			return new IPEndPoint(Dns.GetHostEntry(hostName).AddressList[0],
				port);
		}
	}

	public enum SocketState
	{
		Disconnected,
		Resolving,
		Connecting,
		Connected
	}

	public class SocketAcceptEventArgs : EventArgs
	{
		public SocketAcceptEventArgs(Socket socket)
		{
			Socket = socket;
		}

		public Socket Socket { get; private set; }
	}

	public delegate void SocketAcceptEventHandler(object sender,
		SocketAcceptEventArgs e);

	public class ListenerSocket
	{
		private int port;
		private Socket socket;
		private ISynchronizeInvoke invoke;

		private void Invoke(Delegate d, params object[] args)
		{
			if (invoke != null)
			{
				invoke.Invoke(d, args);
			}
			else
			{
				d.DynamicInvoke(args);
			}
		}

		private void AcceptCallback(IAsyncResult ar)
		{
			try
			{
				Socket client = socket.EndAccept(ar);
				OnAccept(new SocketAcceptEventArgs(client));
				socket.BeginAccept(AcceptCallback, null);
			}
			catch
			{
				Socket server = socket;
				socket = null;
				server.Close();
			}
		}

		protected void OnAccept(SocketAcceptEventArgs e)
		{
			if (Accept != null)
			{
				Invoke(Accept, this, e);
			}
		}

		public ListenerSocket(int port)
		{
			this.port = port;
		}

		public ListenerSocket(int port, ISynchronizeInvoke invoke)
			: this(port)
		{
			this.invoke = invoke;
		}

		public bool Active
		{
			get
			{
				return (socket != null);
			}

			set
			{
				if (value != Active)
				{
					if (value)
					{
						IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
						socket = Network.CreateSocket();
						socket.Bind(endPoint);
						socket.Listen(10);
						socket.BeginAccept(AcceptCallback, null);
					}
					else
					{
						socket.Close();
						socket = null;
					}
				}
			}
		}

		public event SocketAcceptEventHandler Accept;
	}

	public class SocketErrorEventArgs : EventArgs
	{
		public SocketErrorEventArgs(Exception error)
		{
			Error = error;
		}

		public Exception Error { get; private set; }
	}

	public class SocketReceiveEventArgs : EventArgs
	{
		private string text;

		public SocketReceiveEventArgs(byte[] buffer, int size)
		{
			Buffer = buffer;
			Size = size;
			text = string.Empty;
		}

		public byte[] Buffer { get; private set; }

		public int Size { get; private set; }

		public string Text
		{
			get
			{
				if ((text != string.Empty) || (Size < 1))
				{
					return text;
				}
				else
				{
					return Encoding.UTF8.GetString(Buffer, 0, Size);
				}
			}
		}

	}

	public class SocketStateChangeEventArgs : EventArgs
	{
		public SocketStateChangeEventArgs(SocketState priorState,
			SocketState currentState)
		{
			PriorState = priorState;
			CurrentState = currentState;
		}

		public SocketState PriorState { get; private set; }

		public SocketState CurrentState { get; private set; }
	}

	public delegate void SocketErrorEventHandler(object sender,
		SocketErrorEventArgs e);
	public delegate void SocketReceiveEventHandler(object sender,
		SocketReceiveEventArgs e);
	public delegate void SocketStateChangeEventHandler(object sender,
		SocketStateChangeEventArgs e);

	public class ClientSocket
	{
		#region internals
		private byte[] buffer;
		private int size;
		private ISynchronizeInvoke invoke;
		private string hostName;
		private int port;
		private Socket socket;
		private delegate IPHostEntry Resolve(string hostName);

		private void Invoke(Delegate d, params object[] args)
		{
			if (invoke != null)
			{
				invoke.Invoke(d, args);
			}
			else
			{
				d.DynamicInvoke(args);
			}
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				socket.EndConnect(ar);
				UpdateState(SocketState.Connected);
				socket.BeginReceive(buffer, 0, size, SocketFlags.None,
					ReceiveCallback, null);
			}
			catch (Exception e)
			{
				OnError(new SocketErrorEventArgs(e));
			}
		}

		private void ResolveCallback(IAsyncResult ar)
		{
			Resolve resolve = (Resolve)ar.AsyncState;
			try
			{
				IPHostEntry hostEntry = resolve.EndInvoke(ar);
				UpdateState(SocketState.Connecting);
				IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList[0], port);
				socket = Network.CreateSocket();
				socket.BeginConnect(endPoint, ConnectCallback, null);
			}
			catch (Exception e)
			{
				OnError(new SocketErrorEventArgs(e));
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				int bytesReceived = socket.EndReceive(ar);
				if (bytesReceived == 0)
				{
					Disconnect();
					return;
				}
				OnReceive(new SocketReceiveEventArgs(buffer, bytesReceived));
				socket.BeginReceive(buffer, 0, size, SocketFlags.None,
					ReceiveCallback, null);
			}
			catch (Exception e)
			{
				OnError(new SocketErrorEventArgs(e));
			}
		}

		private void SendCallback(IAsyncResult ia)
		{
			try
			{
				socket.EndSend(ia);
			}
			catch (Exception e)
			{
				OnError(new SocketErrorEventArgs(e));
			}
		}

		protected void UpdateState(SocketState state)
		{
			if (this.State != state)
			{
				SocketStateChangeEventArgs e =
					new SocketStateChangeEventArgs(this.State, state);
				this.State = state;
				OnStateChange(e);
			}
		}

		protected void OnReceive(SocketReceiveEventArgs e)
		{
			if (Receive != null)
			{
				Invoke(Receive, this, e);
			}
		}

		protected void OnStateChange(SocketStateChangeEventArgs e)
		{
			if (StateChange != null)
			{
				Invoke(StateChange, this, e);
			}
		}

		protected void OnError(SocketErrorEventArgs e)
		{
			if (Error != null)
			{
				try
				{
					Invoke(Error, this, e);
				}
				finally
				{
					Disconnect();
				}
			}
		}
		#endregion

		public ClientSocket(int bufferSize)
		{
			this.size = bufferSize;
			buffer = new byte[bufferSize];
		}

		public ClientSocket(int bufferSize, ISynchronizeInvoke invoke)
			: this(bufferSize)
		{
			this.invoke = invoke;
		}

		#region methods
		public void Connect(Socket socket)
		{
			Disconnect();
			if (!socket.Connected)
			{
				throw new ArgumentException("Socket not connected", "socket");
			}
			this.socket = socket;
			IPEndPoint endPoint = (IPEndPoint)this.socket.RemoteEndPoint;
			hostName = endPoint.Address.ToString();
			port = endPoint.Port;
			State = SocketState.Connected;
			this.socket.BeginReceive(buffer, 0, size, SocketFlags.None,
				ReceiveCallback, null);
		}

		public void Connect(string hostName, int port)
		{
			Disconnect();
			this.hostName = hostName;
			this.port = port;
			UpdateState(SocketState.Resolving);
			Resolve resolve = Dns.GetHostEntry;
			resolve.BeginInvoke(hostName, ResolveCallback, resolve);
		}

		public void Connect()
		{
			Connect(hostName, port);
		}

		public void Disconnect()
		{
			if (State > SocketState.Disconnected)
			{
				if (socket != null)
				{
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
					socket = null;
				}
				UpdateState(SocketState.Disconnected);
			}
		}

		public void Send(byte[] buffer, int size)
		{
			socket.BeginSend(buffer, 0, size, SocketFlags.None,
				SendCallback, null);
		}

		public void Send(byte[] buffer)
		{
			Send(buffer, buffer.Length);
		}

		public void Send(string buffer)
		{
			Send(System.Text.Encoding.UTF8.GetBytes(buffer));
		}
		#endregion

		#region properties
		public bool Active
		{
			get
			{
				return (State > SocketState.Disconnected);
			}
			set
			{
				if (value != Active)
				{
					if (value)
					{
						Connect();
					}
					else
					{
						Disconnect();
					}
				}
			}
		}

		public string HostName
		{
			get
			{
				return hostName;
			}
			set
			{
				Disconnect();
				hostName = value;
			}
		}

		public int Port
		{
			get
			{
				return port;
			}
			set
			{
				Disconnect();
				port = value;
			}
		}

		public SocketState State { get; private set; }
		#endregion

		#region events
		public event SocketErrorEventHandler Error;
		public event SocketStateChangeEventHandler StateChange;
		public event SocketReceiveEventHandler Receive;
		#endregion
	}

}