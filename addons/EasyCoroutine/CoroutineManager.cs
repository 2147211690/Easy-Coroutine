using Godot;

namespace EasyCoroutine;

public partial class CoroutineManager : Node
{
    public override void _Process(double delta)
    {
        Coroutine.Time(Time.GetTicksMsec());
    }

    public override void _PhysicsProcess(double delta)
    {
        Coroutine.PhysicsFrame();
    }
}