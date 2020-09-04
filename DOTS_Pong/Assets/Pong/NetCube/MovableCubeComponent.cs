using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct MovableCubeComponent : IComponentData
{
    // [GhostDefaultField(100,true)]  // 定义变量的量化 和是否使用插值
    [GhostDefaultField] // 必须添加这个 才会在 Predicting player network id 下拉选择框中添加这个选项
    public int PlayerId;
}