//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\ReadOnlyKeyedCollection.cs
//-----------------------------------------------------------------------------

using System.Collections.ObjectModel;

namespace CuteAnt.Collections
{
  public class ReadOnlyKeyedCollection<TKey, TValue> : ReadOnlyCollection<TValue>
  {
    KeyedCollection<TKey, TValue> m_innerCollection;

    public ReadOnlyKeyedCollection(KeyedCollection<TKey, TValue> innerCollection)
        : base(innerCollection)
    {
      Fx.Assert(innerCollection != null, "innerCollection should not be null");
      m_innerCollection = innerCollection;
    }

    public TValue this[TKey key]
    {
      get { return m_innerCollection[key]; }
    }
  }

}
