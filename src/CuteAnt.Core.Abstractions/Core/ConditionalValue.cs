namespace CuteAnt
{
  /// <summary>ConditionalValue</summary>
  public struct ConditionalValue<TValue>
  {
    public static readonly ConditionalValue<TValue> Empty = new ConditionalValue<TValue>();

    /// <summary>Gets a value indicating whether the current object has a valid value of its underlying type</summary>
    public bool HasValue { get; }

    /// <summary>Gets the value of the current object if it has been assigned a valid underlying value.</summary>
    public TValue Value { get; }

    /// <summary>Initializes a new instance of the class with the given value.</summary>
    /// <param name="hasValue">Indicates whether the value is valid.</param>
    /// <param name="value">The value.</param>
    public ConditionalValue(bool hasValue, TValue value)
    {
      HasValue = hasValue;
      Value = value;
    }
  }
}
