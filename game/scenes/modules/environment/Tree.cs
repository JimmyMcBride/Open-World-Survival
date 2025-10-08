using System;
using Godot;

namespace OpenWorldSurvival.game.scenes.modules.environment;

public partial class Tree : Node3D
{
    private readonly string[] _leafMaterials =
    [
        "res://game/resources/art/materials/green_leaves.tres",
        "res://game/resources/art/materials/yellow_leaves.tres",
        "res://game/resources/art/materials/orange_leaves.tres",
        "res://game/resources/art/materials/red_leaves.tres"
    ];

    [Export] private int _meshIndex;

    public override void _Ready()
    {
        // Get the MeshInstance3D child
        var meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");

        // Create a random number generator
        var random = new Random();

        // Select a random material from the array
        var randomMaterialPath = _leafMaterials[random.Next(_leafMaterials.Length)];

        // Load the material and apply it to surface 0
        var leafMaterial = ResourceLoader.Load<Material>(randomMaterialPath);
        meshInstance.SetSurfaceOverrideMaterial(_meshIndex, leafMaterial);
    }
}