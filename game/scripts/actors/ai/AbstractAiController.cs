using Godot;

namespace OpenWorldSurvival.game.scripts.actors.ai;

public abstract partial class AbstractAiController : CharacterBody3D
{
    public AnimationTree AnimTree;
    public CharacterBody3D Player;
    public float PlayerDistance;
    public abstract void SetIsStopped(bool isStopped);
    public abstract void SetIsLookingAtPlayer(bool isLookingAtPlayer);
    public abstract void SetIsRunning(bool isRunning);
    public abstract void MoveToPosition(Vector3 target, bool adjust = true);
}