using System;
using System.ComponentModel;
using System.Globalization;

namespace CuteAnt
{
  /// <summary>CombGuidConverter</summary>
  public class CombGuidTypeConverter : TypeConverter
  {
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
    {
      if (sourceType == TypeConstants.StringType ||
          sourceType == TypeConstants.GuidType)
      {
        return true;
      }
      return base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc />
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      CombGuid v;
      if (CombGuid.TryParse(value, CombGuidSequentialSegmentType.Comb, out v))
      {
        return v;
      }
      if (CombGuid.TryParse(value, CombGuidSequentialSegmentType.Guid, out v))
      {
        return v;
      }
      return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == TypeConstants.StringType ||
          destinationType == TypeConstants.GuidType ||
          destinationType == TypeConstants.ObjectType)
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    /// <inheritdoc />
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == TypeConstants.StringType)
      {
        return value.ToString();
      }
      if (destinationType == TypeConstants.ObjectType)
      {
        return value;
      }
      if (destinationType == TypeConstants.GuidType)
      {
        return ((CombGuid)value).Value;
      }
      if (destinationType == TypeConstants.ByteArray)
      {
        return ((CombGuid)value).ToByteArray(CombGuidSequentialSegmentType.Comb);
      }

      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}
