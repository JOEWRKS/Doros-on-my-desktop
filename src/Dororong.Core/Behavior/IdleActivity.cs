namespace Dororong.Core.Behavior;

public enum IdleActivityKind { None, Stretch, Beg, Groom }

public readonly record struct IdleActivity(IdleActivityKind Kind, double Age)
{
    public double Duration => Kind switch {
        IdleActivityKind.Stretch => 2.8,
        IdleActivityKind.Beg => 2.4,
        IdleActivityKind.Groom => 3.8,
        _ => 0
    };
}
