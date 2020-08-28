using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct MovableCubeComponent : IComponentData
{
    // [GhostDefaultField(100,true)]  // 定义变量的量化 和是否使用插值
    public int PlayerId;
}