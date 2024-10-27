﻿using System.Reflection;

namespace Money.Common;

public abstract class Enumeration(int value, string name) : IComparable
{
    public string Name { get; } = name;

    public int Value { get; } = value;

    public static implicit operator int(Enumeration enumeration)
    {
        return enumeration.Value;
    }

    public static implicit operator string(Enumeration enumeration)
    {
        return enumeration.Name;
    }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        return fields.Select(f => f.GetValue(null)).Cast<T>();
    }

    public static T FromValue<T>(int value) where T : Enumeration
    {
        T matchingItem = Parse<T, int>(value, nameof(value), item => item.Value == value);
        return matchingItem;
    }

    public static T FromName<T>(string name) where T : Enumeration
    {
        T matchingItem = Parse<T, string>(name, nameof(name), item => item.Name == name);
        return matchingItem;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        bool typeMatches = GetType() == obj.GetType();
        bool valueMatches = Value.Equals(otherValue.Value);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public int CompareTo(Enumeration? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        return other is null ? 1 : Value.CompareTo(other.Value);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (ReferenceEquals(this, obj))
        {
            return 0;
        }

        return obj is Enumeration other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Enumeration)}");
    }

    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        T? matchingItem = GetAll<T>().FirstOrDefault(predicate);

        return matchingItem ?? throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
    }
}
