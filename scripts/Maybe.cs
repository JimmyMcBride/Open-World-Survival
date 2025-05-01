using System;

namespace OpenWorldSurvival.scripts;

public readonly struct Maybe<T>
{
    private readonly T _value;

    public bool IsSome { get; }
    public bool IsNone => !IsSome;


    private Maybe(T value, bool hasValue)
    {
        _value = value;
        IsSome = hasValue;
    }

    public static Maybe<T> None()
    {
        return new Maybe<T>(default, false);
    }

    public static Maybe<T> Some(T value)
    {
        return new Maybe<T>(value, true);
    }

    public T Unwrap()
    {
        if (!IsSome) throw new InvalidOperationException("Attempted to unwrap a None value");
        return _value;
    }

    public T UnwrapOrElse(Func<T> fallback)
    {
        return IsSome ? _value : fallback();
    }

    public T UnwrapOr(T fallback)
    {
        return IsSome ? _value : fallback;
    }

    public T Expect(string errorMessage)
    {
        return IsSome ? _value : throw new InvalidOperationException(errorMessage);
    }

    public T UnwrapOrDefault()
    {
        return IsSome ? _value : default;
    }

    public void Match(Action<T> some, Action none)
    {
        if (IsSome)
            some(_value);
        else
            none();
    }

    // public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    // {
    //     return IsSome ? some(_value) : none();
    // }

    public override string ToString()
    {
        return IsSome ? $"Some({_value})" : "None";
    }
}