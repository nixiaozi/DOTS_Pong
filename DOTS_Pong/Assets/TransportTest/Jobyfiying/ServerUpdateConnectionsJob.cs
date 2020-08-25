
using Unity.Assertions;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

struct ServerUpdateConnectionsJob : IJob
{
    public NetworkDriver driver;
    public NativeList<NetworkConnection> connections;

    public void Execute()
    {
        // CleanUpConnections
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
        // AcceptNewConnections
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
            Debug.Log("Accepted a connection");
        }
    }
}


// What if you create a job that will fan out and run the processing code for all connected clients in parallel? 
// If you look at the documentation for the C# Job System, you can see that there is a IJobParallelFor job type that can handle this scenario
struct ServerUpdateJob : IJobParallelForDefer
{
    public NetworkDriver.Concurrent driver; // NetworkDriver.Concurrent type, this allows you to call the NetworkDriver from multiple threads
    public NativeArray<NetworkConnection> connections;

    public void Execute(int index)
    {
        DataStreamReader stream;
        Assert.IsTrue(connections[index].IsCreated);

        NetworkEvent.Type cmd;
        while ((cmd = driver.PopEventForConnection(connections[index], out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                uint number = stream.ReadUInt();

                Debug.Log("Got " + number + " from the Client adding + 2 to it.");
                number += 2;

                var writer = driver.BeginSend(connections[index]);
                writer.WriteUInt(number);
                driver.EndSend(writer);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client disconnected from server");
                connections[index] = default(NetworkConnection);
            }
        }
    }
}

