using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

public class ServerBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver; // 定义网络驱动
    private NativeList<NetworkConnection> m_Connections; // 定义多个网络连接来保持连接


    // Start is called before the first frame update
    void Start()
    {
        // 数据初始化
        m_Driver = NetworkDriver.Create(); // 创建不带任何参数的网络驱动
        var endpoint = NetworkEndPoint.AnyIpv4; // 创建网络终结点，绑定端口号 AnyIpv4 表示绑定所有支持v4的网卡
        endpoint.Port = 9000;
        if(m_Driver.Bind(endpoint) != 0) // 网络驱动绑定端口号
        {
            Debug.Log("Failed to bind to port 9000");
        }
        else
        {
            m_Driver.Listen();  // 如果绑定端口成功就开始监听端口数据
            // Important: the call to the Listen method sets the NetworkDriver to the Listen state.
            // This means that the NetworkDriver will now actively listen for incoming connections.
        }

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent); // 创建16个永久分配的NetworkConnection
    }

    // Both NetworkDriver and NativeList allocate unmanaged memory and need to be disposed. 
    // 由于networkDriver & NativeList 都被分配在非托管内存上，所有需要手动销毁
    private void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }


    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete(); // 强制同步，在ScheduleUpdate执行完成后执行

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);  // 清除任何过时的链接
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection)) // 如果网络驱动器接收到了新的连接那么，就创建一个新的连接对象
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }

        // 现在我们获取了所有的新连接，现在我需要查询最新的网络驱动事件
        DataStreamReader stream;
        for(int i=0;i<m_Connections.Length;i++)
        {
            if (!m_Connections[i].IsCreated) // 检查连接是否已被创建
                continue;

            NetworkEvent.Type cmd;
            while((cmd= m_Driver.PopEventForConnection(m_Connections[i],out stream)) != NetworkEvent.Type.Empty)//如果网络驱动获取了非空事件，就写入事件流
            {
                // We are now ready to process events. Lets start with the Data event
                if(cmd == NetworkEvent.Type.Data)
                {
                    uint number = stream.ReadUInt();  // 尝试读取UInt类型数据
                    Debug.Log("Got " + number + " from the Client adding + 2 to it.");
                    number += 2;

                    // NetworkPipeline.Null 表示使用一个未使用过的管道发送数据，同时也表示它不会指定发送的管道
                    var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]); // 开始发送数据
                    writer.WriteUInt(number);
                    m_Driver.EndSend(writer); // 已经发送完全部数据

                }
                else if(cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection); // 重置已断开的连接
                }

            }


        }


    }
}
