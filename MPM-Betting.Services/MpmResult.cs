using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using LanguageExt.TypeClasses;

namespace MPM_Betting.Services;

public readonly struct MpmResult<T> : IEquatable<MpmResult<T>>, IComparable<MpmResult<T>>
{
    private readonly ResultState m_State;
    public readonly T Value;
    private readonly Exception m_Exception;

    internal Exception Exception => m_Exception ?? BottomException.Default;

    /// <summary>Constructor of a concrete value</summary>
    /// <param name="value"></param>
    public MpmResult(T value)
    {
        m_State = ResultState.Success;
        Value = value;
        m_Exception = null!;
    }

    /// <summary>Constructor of an error value</summary>
    /// <param name="e"></param>
    public MpmResult(Exception e)
    {
        m_State = ResultState.Faulted;
        m_Exception = e;
        Value = default!;
    }

    [Pure]
    public static implicit operator MpmResult<T>(T value)
    {
        return new MpmResult<T>(value);
    }
    
    [Pure]
    public static implicit operator MpmResult<T>(Exception e)
    {
        return new MpmResult<T>(e);
    }

    /// <summary>True if the result is faulted</summary>
    [Pure]
    public bool IsFaulted => m_State == ResultState.Faulted;

    /// <summary>True if the struct is in an invalid state</summary>
    [Pure]
    public bool IsBottom
    {
        get
        {
            if (m_State != ResultState.Faulted)
                return false;
            return m_Exception is null or BottomException;
        }
    }

    /// <summary>True if the struct is in a success</summary>
    [Pure]
    public bool IsSuccess => m_State == ResultState.Success;

    /// <summary>Convert the value to a showable string</summary>
    [Pure]
    public override string ToString()
    {
        if (IsFaulted)
            return m_Exception?.ToString() ?? "(Bottom)";
        var a = Value;
        ref var local = ref a;
        return (local != null ? local.ToString() : null) ?? "(null)";
    }

    /// <summary>Equality check</summary>
    [Pure]
    public bool Equals(MpmResult<T> other)
    {
        return new MpmEqResult<T>().Equals(this, other);
    }

    /// <summary>Equality check</summary>
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is MpmResult<T> other && Equals(other);
    }

    /// <summary>Get hash code for bound value</summary>
    [Pure]
    public override int GetHashCode()
    {
        return new MpmEqResult<T>().GetHashCode(this);
    }

    [Pure]
    public static bool operator ==(MpmResult<T> a, MpmResult<T> b)
    {
        return new MpmEqResult<T>().Equals(a, b);
    }

    [Pure]
    public static bool operator !=(MpmResult<T> a, MpmResult<T> b)
    {
        return !(a == b);
    }

    [Pure]
    public T IfFail(T defaultValue)
    {
        return !IsFaulted ? Value : defaultValue;
    }

    [Pure]
    public T IfFail(Func<Exception, T> f)
    {
        return !IsFaulted ? Value : f(Exception);
    }

    public Unit IfFail(Action<Exception> f)
    {
        if (IsFaulted)
            f(Exception);
        return Prelude.unit;
    }

    public Unit IfSuc(Action<T> f)
    {
        if (IsSuccess)
            f(Value);
        return Prelude.unit;
    }
    
    [Pure]
    public TR Match<TR>(Func<T, TR> suc, Func<Exception, TR> fail)
    {
        return !IsFaulted ? suc(Value) : fail(Exception);
    }

    [Pure]
    internal OptionalResult<T> ToOptional()
    {
        return !IsFaulted
            ? new OptionalResult<T>(Prelude.Optional(Value))
            : new OptionalResult<T>(Exception);
    }

    [Pure]
    public MpmResult<TB> Map<TB>(Func<T, TB> f)
    {
        return !IsFaulted ? new MpmResult<TB>(f(Value)) : new MpmResult<TB>(Exception);
    }

    [Pure]
    public async Task<MpmResult<TB>> MapAsync<TB>(Func<T, Task<TB>> f)
    {
        var mpmResult = IsFaulted ? new MpmResult<TB>(Exception) : new MpmResult<TB>(await f(Value));
        return mpmResult;
    }

    [Pure]
    public int CompareTo(MpmResult<T> other)
    {
        return new MpmOrdResult<T>().Compare(this, other);
    }

    [Pure]
    public static bool operator <(MpmResult<T> a, MpmResult<T> b)
    {
        return a.CompareTo(b) < 0;
    }

    [Pure]
    public static bool operator <=(MpmResult<T> a, MpmResult<T> b)
    {
        return a.CompareTo(b) <= 0;
    }

    [Pure]
    public static bool operator >(MpmResult<T> a, MpmResult<T> b)
    {
        return a.CompareTo(b) > 0;
    }

    [Pure]
    public static bool operator >=(MpmResult<T> a, MpmResult<T> b)
    {
        return a.CompareTo(b) >= 0;
    }
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MpmOrdResult<TA> :
    Ord<MpmResult<TA>>
{
    public int Compare(MpmResult<TA> x, MpmResult<TA> y)
    {
        return x.IsBottom switch
        {
            true when y.IsBottom => 0,
            true when !y.IsBottom => -1,
            false when y.IsBottom => 1,
            _ => x.IsFaulted switch
            {
                true when y.IsFaulted => 0,
                true when !y.IsFaulted => -1,
                _ => !x.IsFaulted && y.IsFaulted ? 1 : new OrdDefault<TA>().Compare(x.Value, y.Value)
            }
        };
    }

    public bool Equals(MpmResult<TA> x, MpmResult<TA> y)
    {
        return (x.IsBottom && y.IsBottom) ||
               (x.IsFaulted && y.IsFaulted && new EqTypeInfo().Equals(x.Exception.GetType().GetTypeInfo(),
                   y.Exception.GetType().GetTypeInfo())) || new EqDefault<TA>().Equals(x.Value, y.Value);
    }

    public int GetHashCode(MpmResult<TA> x)
    {
        if (x.IsBottom)
            return -2;
        if (x.IsFaulted)
            return -1;
        TA a = x.Value;
        ref TA local = ref a;
        return local == null ? 0 : local.GetHashCode();
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> EqualsAsync(MpmResult<TA> x, MpmResult<TA> y)
    {
        return Equals(x, y).AsTask();
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> GetHashCodeAsync(MpmResult<TA> x)
    {
        return GetHashCode(x).AsTask();
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> CompareAsync(MpmResult<TA> x, MpmResult<TA> y)
    {
        return Compare(x, y).AsTask();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MpmEqResult<TA> :
    Eq<MpmResult<TA>>
{
    [Pure]
    public bool Equals(MpmResult<TA> x, MpmResult<TA> y)
    {
        return (x.IsBottom && y.IsBottom) ||
               (x.IsFaulted && y.IsFaulted && new EqTypeInfo().Equals(x.Exception.GetType().GetTypeInfo(),
                   y.Exception.GetType().GetTypeInfo())) || new EqDefault<TA>().Equals(x.Value, y.Value);
    }

    [Pure]
    public int GetHashCode(MpmResult<TA> x)
    {
        return new MpmHashableResult<TA>().GetHashCode(x);
    }

    [Pure]
    public Task<bool> EqualsAsync(MpmResult<TA> x, MpmResult<TA> y)
    {
        return Equals(x, y).AsTask();
    }

    [Pure]
    public Task<int> GetHashCodeAsync(MpmResult<TA> x)
    {
        return GetHashCode(x).AsTask();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MpmHashableResult<TA> : Hashable<MpmResult<TA>>
{
    /// <summary>Get hash code of the value</summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    public int GetHashCode(MpmResult<TA> x)
    {
        if (x.IsBottom)
            return -2;
        if (x.IsFaulted)
            return -1;
        var a = x.Value;
        ref var local = ref a;
        return local == null ? 0 : local.GetHashCode();
    }

    /// <summary>Get hash code of the value</summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<int> GetHashCodeAsync(MpmResult<TA> x)
    {
        return GetHashCode(x).AsTask();
    }
}