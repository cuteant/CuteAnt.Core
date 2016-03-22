using System;

namespace CuteAnt
{
  internal class AssemblyInfo
  {
    /// <summary>Copyright (c) 2000-2016 CuteAnt Development Team</summary>
    public const String AssemblyCopyright = "Copyright (c) 2000-2016 CuteAnt Development Team";

    /// <summary>CuteAnt Development Team</summary>
    public const String AssemblyCompany = "CuteAnt Development Team(cuteant@outlook.com)";

    /// <summary>CuteAnt Development Team</summary>
    public const String AssemblyEmail = "cuteant@outlook.com";

    /// <summary>CuteAnt</summary>
    public const String AssemblyProduct = "CuteAnt";

    /// <summary>8df3f1eee85cc956</summary>
    public const String PublicKeyToken = "8df3f1eee85cc956";

    /// <summary>
    /// Use in InternalsVisibleToAttribute
    /// 00240000048000009400000006020000002400005253413100040000010001003df2cefc3e3c196195f046768979f5998131a23270da7485c84d0e46175140c4227e93fe392829d51d1e1ffbe0d6edb3bb0b2b05556f829f2f1a184f23ce052e2b2134ba0ae7aa9143a7959cea16accb18d1417bf48dabac10c2c0828ede943c5960e85713ca29eea555959ea6dbdd41d1000bf62da370883c4dc5c3508a22df
    /// </summary>
    public const String PublicKey = "00240000048000009400000006020000002400005253413100040000010001003df2cefc3e3c196195f046768979f5998131a23270da7485c84d0e46175140c4227e93fe392829d51d1e1ffbe0d6edb3bb0b2b05556f829f2f1a184f23ce052e2b2134ba0ae7aa9143a7959cea16accb18d1417bf48dabac10c2c0828ede943c5960e85713ca29eea555959ea6dbdd41d1000bf62da370883c4dc5c3508a22df";

    /// <summary>neutral</summary>
    public const String Culture = "neutral";

#if NET40
    /// <summary>4</summary>
    public const String NETVersion = "4";
#elif NET451 || DNX451
    /// <summary>5</summary>
    public const String NETVersion = "5";
#elif NET46
    /// <summary>6</summary>
    public const String NETVersion = "6";
#endif

    /// <summary>1</summary>
    public const String VersionMajor = "2";

    /// <summary>2</summary>
    public const String VersionMinor = "2";

    /// <summary>2</summary>
    public const String FileVersionMinor = "2";

    /// <summary>
    /// 1.x
    /// - x代表NetFX版本
    /// </summary>
    public const String VersionShort = VersionMajor + "." + NETVersion;

    /// <summary>
    /// 1.x.
    /// - x代表NetFX版本
    /// </summary>
    public const String Version = VersionShort + "."; // + VersionMinor;

    /// <summary>
    /// 1.x.2.168
    /// - x代表NetFX版本
    /// </summary>
    public const String StaticVersion = VersionShort + "." + VersionMinor + ".168";

    /// <summary></summary>
    public static class _
    {
      public const string Beta1 = "-beta1";
      public const string Beta2 = "-beta2";
      public const string Beta3 = "-beta3";
      public const string Beta4 = "-beta4";
      public const string Beta5 = "-beta5";
      public const string Beta6 = "-beta6";
      public const string Beta7 = "-beta7";
      public const string Beta8 = "-beta8";
      public const string RC1 = "-rc1";
      public const string RC2 = "-rc2";
      public const string RC3 = "-rc3";
      public const string RC4 = "-rc4";
      public const string RC5 = "-rc5";
      public const string RC6 = "-rc6";
      public const string RC7 = "-rc7";
      public const string RC8 = "-rc8";
      public const string Final = "-final";
    }

    /// <summary>
    /// 1.x.2.168
    /// - x代表NetFX版本
    /// </summary>
    public const String InformationalVersion = VersionMajor + "." + VersionMinor + ".0" + _.RC2;

    /// <summary>1.2
    /// - x代表NetFX版本
    /// </summary>
    public const String FileVersion = VersionShort + "." + FileVersionMinor;

    ///// <summary>
    ///// v1.x
    ///// - x代表NetFX版本
    ///// </summary>
    //public const String VSuffixWithoutSeparator = "v" + VersionShort;

    /// <summary>
    /// .v1.x
    /// - x代表NetFX版本
    /// </summary>
    //public const String VSuffix = "." + VSuffixWithoutSeparator;
    public const String VSuffix = "";

    /// <summary>
    /// .v1.x.Design
    /// - x代表NetFX版本
    /// </summary>
    public const String VSuffixDesign = VSuffix + ".Design";

    /// <summary>
    /// 1x
    /// - x代表NetFX版本
    /// </summary>
    //public const String VSuffixWin = VersionMajor + NETVersion;
    public const String VSuffixWin = "";

    /// <summary>
    /// .v1.x, PublicKey=00240000048000009400000006020000002400005253413100040000010001003df2cefc3e3c196195f046768979f5998131a23270da7485c84d0e46175140c4227e93fe392829d51d1e1ffbe0d6edb3bb0b2b05556f829f2f1a184f23ce052e2b2134ba0ae7aa9143a7959cea16accb18d1417bf48dabac10c2c0828ede943c5960e85713ca29eea555959ea6dbdd41d1000bf62da370883c4dc5c3508a22df
    /// - x代表NetFX版本
    /// </summary>
    public const String PublicKeyString = VSuffix + ", PublicKey=" + PublicKey;

    /// <summary>
    /// .v1.x.Design, PublicKey=00240000048000009400000006020000002400005253413100040000010001003df2cefc3e3c196195f046768979f5998131a23270da7485c84d0e46175140c4227e93fe392829d51d1e1ffbe0d6edb3bb0b2b05556f829f2f1a184f23ce052e2b2134ba0ae7aa9143a7959cea16accb18d1417bf48dabac10c2c0828ede943c5960e85713ca29eea555959ea6dbdd41d1000bf62da370883c4dc5c3508a22df
    /// - x代表NetFX版本
    /// </summary>
    public const String DesignPublicKeyString = VSuffixDesign + ", PublicKey=" + PublicKey;

    /// <summary>
    /// , Version=1.x.2.168, Culture=neutral, PublicKeyToken=8df3f1eee85cc956
    /// - x代表NetFX版本
    /// </summary>
    public const String AssemblyInfoSuffix = ", Version=" + StaticVersion + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;

    /// <summary>
    /// .v1.x, Version=1.x.2.168, Culture=neutral, PublicKeyToken=8df3f1eee85cc956
    /// - x代表NetFX版本
    /// </summary>
    public const String AssemblyInfoVSuffix = VSuffix + AssemblyInfoSuffix;

    /// <summary>
    /// .v1.x.Design, Version=1.x.2.168, Culture=neutral, PublicKeyToken=8df3f1eee85cc956
    /// - x代表NetFX版本
    /// </summary>
    public const String AssemblyInfoVSuffixDesign = VSuffixDesign + AssemblyInfoSuffix;
  }
}
