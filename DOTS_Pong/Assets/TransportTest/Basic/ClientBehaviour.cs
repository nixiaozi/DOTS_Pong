using UnityEngine;

using Unity.Networking.Transport;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection; // 这里定义的是单个连接
    public bool m_Done;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;  // 表示本机ipv4地址：127.0.0.1
        endpoint.Port = 9000; // 表示要连接的端口号
        m_Connection = m_Driver.Connect(endpoint); // l连接ip端口
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete(); // ScheduleUpdate 执行完成后执行

        if (!m_Connection.IsCreated) // 判断连接是否成功
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream; 
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty) // PopEvent 表示单个连接的事件 相对于 PopEventForConnection
        {
            if (cmd == NetworkEvent.Type.Connect) // 表示当前连接状态是连接成功
            {
                Debug.Log("We are now connected to the server");

                uint value = 1;
                var writer = m_Driver.BeginSend(m_Connection); // 连接成功后开始发送数据
                writer.WriteUInt(value);
                m_Driver.EndSend(writer);
            }
            else if (cmd == NetworkEvent.Type.Data) // 表示接收到服务端发来的数据
            {
                uint value = stream.ReadUInt();  
                Debug.Log("Got the value = " + value + " back from the server");
                m_Done = true; // 接收数据完成后，标识修改为已完成，并请求关闭连接
                m_Connection.Disconnect(m_Driver); // 请求关闭连接
                m_Connection = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect) // 服务端发送消息，同一关闭连接
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection); // 本地连接关闭
            }
        }
    }
}