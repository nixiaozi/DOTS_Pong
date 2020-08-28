using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Burst;

public struct CubeInput : ICommandData<CubeInput>
{
    public uint Tick => tick;
    public uint tick;
    public int horizontal;
    public int vertical;

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        horizontal = reader.ReadInt();
        vertical = reader.ReadInt();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(horizontal);
        writer.WriteInt(vertical);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, CubeInput baseline,
        NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    public void Serialize(ref DataStreamWriter writer, CubeInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }
}

public class NetCubeSendCommandSystem : CommandSendSystem<CubeInput>
{
}
public class NetCubeReceiveCommandSystem : CommandReceiveSystem<CubeInput>
{
}

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SampleCubeInput : SystemBase
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>(); // 创建一个组件对象准备传输数据
        RequireSingletonForUpdate<EnableNetCubeGhostReceiveSystemComponent>(); // 创建一个组件对象准备接收对象
    }

    /*protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<CubeInput>().ForEach((Entity ent, ref MovableCubeComponent cube) =>
            {
                if (cube.PlayerId == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<CubeInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            });
            return;
        }
        var input = default(CubeInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;
        if (Input.GetKey("a"))
            input.horizontal -= 1;
        if (Input.GetKey("d"))
            input.horizontal += 1;
        if (Input.GetKey("s"))
            input.vertical -= 1;
        if (Input.GetKey("w"))
            input.vertical += 1;
        var inputBuffer = EntityManager.GetBuffer<CubeInput>(localInput);
        inputBuffer.AddCommandData(input);
    }*/

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        var entityManager = EntityManager;
        //if the singleton is not set it means that we need to setup our cube
        if (localInput == Entity.Null)
        {
            //find the cube that we control
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities
                .WithStructuralChanges() // SystemBase 才会有这个扩展
                .WithNone<CubeInput>()
                .ForEach((Entity ent, ref MovableCubeComponent cube) =>
            {
                if (cube.PlayerId == localPlayerId)
                {
                    //add our input buffer and set the singleton to our cube
                    UnityEngine.Debug.Log("Added input buffer");
                    entityManager.AddBuffer<CubeInput>(ent);
                    entityManager.SetComponentData(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            }).Run();
            return;
        }
        var input = default(CubeInput);
        //set our tick so we can roll it back
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;
        //the reason that we are adding instead of just setting the values is so that opposite keys cancel each other out
        if (Input.GetKey("a"))
            input.horizontal -= 1;
        if (Input.GetKey("d"))
            input.horizontal += 1;
        if (Input.GetKey("s"))
            input.vertical -= 1;
        if (Input.GetKey("w"))
            input.vertical += 1;
        //add our input to the buffer
        var inputBuffer = EntityManager.GetBuffer<CubeInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}




