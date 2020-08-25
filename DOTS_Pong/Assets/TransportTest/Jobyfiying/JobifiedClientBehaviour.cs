using UnityEngine;

using Unity.Jobs;
using Unity.Collections;
using Unity.Networking.Transport;

public class JobifiedClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NativeArray<NetworkConnection> m_Connection;
    public NativeArray<byte> m_Done;  // 因为bool并不是 blittable type 类型的数据，需要使用byte代替，因为NativeArray<T> T 只能使用blittable type

    public JobHandle ClientJobHandle;

    void Start()
    {
        m_Driver = NetworkDriver.Create();

        m_Connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent); // 初始化数据，一个元素
        m_Done = new NativeArray<byte>(1, Allocator.Persistent);
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection[0] = m_Driver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        ClientJobHandle.Complete();
        m_Connection.Dispose();
        m_Driver.Dispose();
        m_Done.Dispose();
    }

    void Update()
    {
        // You want to make sure(again) that before you start running your new frame, we check that the last frame is complete.
        // Instead of calling m_Driver.ScheduleUpdate().Complete(), use the JobHandle and call ClientJobHandle.Complete().
        ClientJobHandle.Complete();
        // To chain your job, start by creating a job struct:
        var job = new ClientUpdateJob
        {
            driver = m_Driver,
            connection = m_Connection,
            done = m_Done
        };
        ClientJobHandle = m_Driver.ScheduleUpdate(); //执行网络驱动ScheduleUpdate 方法，并返回值到ClientJobHandle
        ClientJobHandle = job.Schedule(ClientJobHandle); // job.Schedule 依赖于 ClientJobHandle
    }
}

struct ClientUpdateJob : IJob
{
    public NetworkDriver driver;
    public NativeArray<NetworkConnection> connection;  // 因为是异步模式可能出现问题
    public NativeArray<byte> done;

    public void Execute()   // Execute 定义了要执行的任务是什么
    {
        if (!connection[0].IsCreated) // connection[0]表示第一个连接，是不是创建成功了
        {
            if (done[0] != 1)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = connection[0].PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");

                uint value = 1;
                var writer = driver.BeginSend(connection[0]);
                writer.WriteUInt(value);
                driver.EndSend(writer);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log("Got the value = " + value + " back from the server");
                // And finally change the `done[0]` to `1`
                done[0] = 1;
                connection[0].Disconnect(driver);
                connection[0] = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                connection[0] = default(NetworkConnection);
            }
        }
    }
}

