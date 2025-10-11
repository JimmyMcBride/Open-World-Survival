using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.engine.godot;
using OpenWorldSurvival.game.globals.signals;

namespace OpenWorldSurvival.game.scripts.actors.player;

public partial class EnemyInRangeDetector : RayCast3D
{
    private float _attackRange = 1.5f;
    private bool _lastCollisionState;
    private CharacterBody3D _ownerBody;

    [Export]
    public float AttackRange
    {
        get => _attackRange;
        set
        {
            _attackRange = value;
            UpdateRaycastLength();
        }
    }

    public override void _Ready()
    {
        _ownerBody = GetParent().GetParent().GetParent<CharacterBody3D>();
        // Ensure the raycast length is set when the node is initialized
        UpdateRaycastLength();
    }

    public override void _PhysicsProcess(double delta)
    {
        var isCollidingWithValidBody = false;

        // Check if the raycast is colliding
        if (IsColliding())
        {
            var collider = GetCollider();
            collider.HasMethod("SetIsLookingAtPlayer");
            // Ensure the collider is a body and not the owner's CharacterBody3D
            if (collider.HasMethod("SetIsLookingAtPlayer")) isCollidingWithValidBody = true;
        }

        // Emit signal only if the state has changed to avoid spamming
        if (isCollidingWithValidBody != _lastCollisionState)
        {
            Log.Debug($"****Changed collision state to {isCollidingWithValidBody}****");
            _lastCollisionState = isCollidingWithValidBody;
            GlobalSignals.Instance.EmitOnEnemyInRange(isCollidingWithValidBody);
        }
    }

    private void UpdateRaycastLength()
    {
        // Set the TargetPosition to extend along the local Z-axis (forward for RayCast3D)
        TargetPosition = TargetPosition.SetZ(-AttackRange);
    }
}