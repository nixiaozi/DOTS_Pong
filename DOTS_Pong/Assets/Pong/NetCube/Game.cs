using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using UnityEngine;

/*// Control system updating in the default world
[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
public class Game : SystemBase  //  ComponentSystem  to  SystemBase
{
    // Singleton component to trigger connections once from a control system
    struct InitGameComponent : IComponentData
    {
    }
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitGameComponent>();
        // Create singleton, require singleton for update so system runs once
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    protected override void OnUpdate()
    {
        // Destroy singleton to prevent system from running again
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());
        foreach (var world in World.All)
        {
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            {
                // Client worlds automatically connect to localhost
                NetworkEndPoint ep = NetworkEndPoint.LoopbackIpv4;
                ep.Port = 7979;
                network.Connect(ep);
            }
#if UNITY_EDITOR
            else if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            {
                // Server world automatically listens for connections from any host
                NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
                ep.Port = 7979;
                network.Listen(ep);
            }
#endif
        }
    }
}*/

[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
public class Game : SystemBase
{
    // Singleton component to trigger connections once from a control system
    struct InitGameComponent : IComponentData
    {
    }
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitGameComponent>();
        // Create singleton, require singleton for update so system runs once
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    protected override void OnUpdate()
    {
        // Destroy singleton to prevent system from running again
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());
        foreach (var world in World.All)
        {
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            {
                //to chose an IP instead of just always connecting to localhost uncomment the next line and delete the other two lines of code
                //NetworkEndPoint.TryParse("IP address here", 7979, out NetworkEndPoint ep);
                NetworkEndPoint ep = NetworkEndPoint.LoopbackIpv4;
                ep.Port = 7979;

                network.Connect(ep);
            }
#if UNITY_EDITOR
            else if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            {
                // Server world automatically listens for connections from any host
                NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
                ep.Port = 7979;
                network.Listen(ep);
            }
#endif
        }
    }
}


/*[BurstCompile]
public struct GoInGameRequest : IRpcCommand
{
    public void Deserialize(ref DataStreamReader reader)
    {
    }

    public void Serialize(ref DataStreamWriter writer)
    {
    }
    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<GoInGameRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);
    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}*/
[BurstCompile]
public struct GoInGameRequest : IRpcCommand
{
    public void Deserialize(ref DataStreamReader reader)
    {
    }

    public void Serialize(ref DataStreamWriter writer)
    {
    }
    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<GoInGameRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);
    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

// 在 Client World中运行
// The system that makes the RPC request component transfer
public class GoInGameRequestSystem : RpcCommandRequestSystem<GoInGameRequest>
{
}

/*// When client has a connection with network id, go in game and tell server to also go in game
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class GoInGameClientSystem : SystemBase
{
    protected override void OnCreate()
    {
    }

    //protected override void OnUpdate()
    //{
    //    Entities.WithNone<NetworkStreamInGame>().ForEach((Entity ent, ref NetworkIdComponent id) =>
    //    {
    //        PostUpdateCommands.AddComponent<NetworkStreamInGame>(ent);
    //        var req = PostUpdateCommands.CreateEntity();
    //        PostUpdateCommands.AddComponent<GoInGameRequest>(req);
    //        PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
    //    });
    //}

    private EntityQuery query;

    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
        // we added the with structural changes so that we can add compontes and the with none to
        // make sure we don't aend the request if we already have a connection to the server
        Entities.WithStructuralChanges()
            .WithNone<NetworkStreamInGame>()
            .WithStoreEntityQueryInField(ref query)  // 获取使用的 EntityQuery  
            .ForEach((Entity ent, ref NetworkIdComponent id) =>
            {
                // we add a network stream in game component so that this code is only run once 
                entityManager.AddComponent<NetworkStreamInGame>(ent);

                // create an entity to hold our request, requests are automatically sent when they are
                // detected on an entity making it really simple to send them.
                var req = entityManager.CreateEntity();

                // add the rpc request component, this is what tells the sending system to send it
                entityManager.AddComponent<SendRpcCommandRequestComponent>(req);

                // add the entity with the network components as our target
                entityManager.SetComponentData(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
            }).Run();


    }

}
*/


// When client has a connection with network id, go in game and tell server to also go in game
//this update in group makes sure this code only runs on the client
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class GoInGameClientSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
        //we addd the with structural changes so that we can add components and the with none to make sure we don't send the request if we already have a connection to the server
        Entities.WithStructuralChanges().WithNone<NetworkStreamInGame>().ForEach((Entity ent, ref NetworkIdComponent id) =>
        {
            //we add a network stream in game component so that this code is only run once
            entityManager.AddComponent<NetworkStreamInGame>(ent);
            //create an entity to hold our request, requests are automatically sent when they are detected on an entity making it really simple to send them.
            var req = entityManager.CreateEntity();
            //add our go in game request
            entityManager.AddComponent<GoInGameRequest>(req);
            //add the rpc request component, this is what tells the sending system to send it
            entityManager.AddComponent<SendRpcCommandRequestComponent>(req);
            //add the entity with the network components as our target.
            entityManager.SetComponentData(req, new SendRpcCommandRequestComponent { TargetConnection = ent });

            // Camera add directly test 
/*            Camera camera = new Camera();   // 出现为空的错误    
            camera.enabled = true;
            GameManager.Instantiate<Camera>(camera);*/


        }).Run();
    }
}




/*// When server receives go in game request, go in game and delete request
// 也可以编写自己的逻辑，需要等到所有用户全部连接后才进入游戏
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GoInGameServerSystem : SystemBase
{
    //protected override void OnUpdate()
    //{
    //    Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref GoInGameRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
    //    {
    //        PostUpdateCommands.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
    //        UnityEngine.Debug.Log(String.Format("Server setting connection {0} to in game", EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value));
    //        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
    //        var ghostId = NetCubeGhostSerializerCollection.FindGhostType<CubeSnapshotData>();
    //        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
    //        var player = EntityManager.Instantiate(prefab);

    //        EntityManager.SetComponentData(player, new MovableCubeComponent { PlayerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value });
    //        PostUpdateCommands.AddBuffer<CubeInput>(player);

    //        PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent { targetEntity = player });

    //        PostUpdateCommands.DestroyEntity(reqEnt);
    //    });
    //}


    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;

        // the with none is to make sure that we don't run this system on rpcs that
        // we are sending,only on ones that we are receiving.
        Entities
            .WithStructuralChanges().WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity reqEnt, ref GoInGameRequest req,
                ref ReceiveRpcCommandRequestComponent reqSrc) =>
            {
                // we add a net work connection to the component on our side
                entityManager.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
                UnityEngine.Debug.Log(String.Format("Server setting connection {0} to in game",
                    EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value));

                var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();

                // if you get errors in the code make sure you have generated the ghost collection
                // and ghost code using their authoring components and that the names are set correctly 
                // when you do so.

                var ghostId = NetCubeGhostSerializerCollection.FindGhostType<CubeSnapshotData>();
                var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>
                    (ghostCollection.serverPrefabs)[ghostId].Value;
                var player = entityManager.Instantiate(prefab);

                entityManager.SetComponentData(player, new MovableCubeComponent
                {
                    PlayerId = entityManager
                        .GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value
                });

                // if you are copy pasting this article in order don't worry if you get an error
                // about cube input not existing as we are going to code that next, this just
                // adds the buffer so that we can receive input from the player.If you want to 
                // test your code at this point just comment it out
                entityManager.AddBuffer<CubeInput>(player);

                entityManager.SetComponentData(reqSrc.SourceConnection, new
                    CommandTargetComponent
                { targetEntity = player });

                entityManager.DestroyEntity(reqEnt);

                UnityEngine.Debug.Log("Spawned Player");
            }).Run();

    }


}
*/


// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]//make sure this only runs on the server
public class GoInGameServerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
        //the with none is to make sure that we don't run this system on rpcs that we are sending, only on ones that we are receiving. 
        Entities
            .WithStructuralChanges()
            .WithNone<SendRpcCommandRequestComponent>().ForEach(
            (Entity reqEnt, // reqEnt 当前迭代的实体
                ref GoInGameRequest req, // req 当前迭代实体的 GoInGameRequest 组件
                ref ReceiveRpcCommandRequestComponent reqSrc
                ) => 
        {
            //we add a network connection to the component on our side
            entityManager.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
            UnityEngine.Debug.Log(System.String.Format("Server setting connection {0} to in game", EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value));

            var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
            //if you get errors in this code make sure you have generated the ghost collection and ghost code using their authoring components and that the names are set correctly when you do so.
            var ghostId = NetCubeGhostSerializerCollection.FindGhostType<CubeSnapshotData>();
            var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
            //spawn in our player
            var player = entityManager.Instantiate(prefab);

            entityManager.SetComponentData(player, new MovableCubeComponent { PlayerId = entityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value });
            //if you are copy pasting this article in order don't worry if you get an error about cube input not existing as we are going to code that next, this just adds the buffer so that we can receive input from the player. If you want to test your code at this point just comment it out
            entityManager.AddBuffer<CubeInput>(player); 

            entityManager.SetComponentData(reqSrc.SourceConnection, new CommandTargetComponent { targetEntity = player });

            entityManager.DestroyEntity(reqEnt);
            Debug.Log("Spawned Player");
        }).Run();
    }
}