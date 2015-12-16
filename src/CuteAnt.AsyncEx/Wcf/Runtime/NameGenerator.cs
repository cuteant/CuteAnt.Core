//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Threading;

namespace CuteAnt.AsyncEx
{
  public sealed class NameGenerator
  {
    static NameGenerator s_nameGenerator = new NameGenerator();
    long m_id;
    string m_prefix;

    NameGenerator()
    {
      m_prefix = string.Concat("_", Guid.NewGuid().ToString().Replace('-', '_'), "_");
    }

    public static string Next()
    {
      long nextId = Interlocked.Increment(ref s_nameGenerator.m_id);
      return s_nameGenerator.m_prefix + nextId.ToString(CultureInfo.InvariantCulture);
    }
  }
}
