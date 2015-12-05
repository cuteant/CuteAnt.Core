// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Collections.ObjectModel
{
  /// <summary>A class that inherits from Collection of T but also exposes its underlying data as List of T for performance.</summary>
  internal sealed class ListWrapperCollection<T> : Collection<T>
  {
    private readonly List<T> _items;

    public ListWrapperCollection()
      : this(new List<T>())
    {
    }

    public ListWrapperCollection(List<T> list)
      : base(list)
    {
      _items = list;
    }

    public List<T> ItemsList
    {
      get { return _items; }
    }
  }
}
