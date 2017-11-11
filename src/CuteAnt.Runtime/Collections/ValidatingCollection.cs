//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\Collections\ValidatingCollection.cs
//------------------------------------------------------------

using System;
using System.Collections.ObjectModel;

namespace CuteAnt.Collections
{
  // simple helper class to allow passing in a func that performs validations of
  // acceptible values
  public class ValidatingCollection<T> : Collection<T>
  {
    public ValidatingCollection()
    {
    }

    public Action<T> OnAddValidationCallback { get; set; }
    public Action OnMutateValidationCallback { get; set; }

    private void OnAdd(T item)
    {
      if (OnAddValidationCallback != null)
      {
        OnAddValidationCallback(item);
      }
    }

    private void OnMutate()
    {
      if (OnMutateValidationCallback != null)
      {
        OnMutateValidationCallback();
      }
    }

    protected override void ClearItems()
    {
      OnMutate();
      base.ClearItems();
    }

    protected override void InsertItem(int index, T item)
    {
      OnAdd(item);
      base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
      OnMutate();
      base.RemoveItem(index);
    }

    protected override void SetItem(int index, T item)
    {
      OnAdd(item);
      OnMutate();
      base.SetItem(index, item);
    }
  }
}