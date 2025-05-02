#nullable enable
using Godot;
using OpenWorldSurvival.engine.core;

namespace OpenWorldSurvival.engine.godot;

public static class Extensions
{
    public static Vector3 SetX(this Vector3 vector, float x)
    {
        return new Vector3(x, vector.Y, vector.Z);
    }

    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.X, y, vector.Z);
    }

    public static Vector3 SetZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.X, vector.Y, z);
    }

    public static Vector3 SetXy(this Vector3 vector, float x, float y)
    {
        return new Vector3(x, y, vector.Z);
    }

    public static Vector3 SetXz(this Vector3 vector, float x, float z)
    {
        return new Vector3(x, vector.Y, z);
    }

    public static Vector3 SetYz(this Vector3 vector, float y, float z)
    {
        return new Vector3(vector.X, y, z);
    }

    public static Maybe<T> ToMaybe<T>(this T? value) where T : class
    {
        return value != null ? Maybe<T>.Some(value) : Maybe<T>.None();
    }
}