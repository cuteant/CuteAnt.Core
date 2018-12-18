﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace CuteAnt.Pool.Tests
{
  public class DefaultObjectPoolTest
  {
    [Fact]
    public void ConcurrentObjectPool_GetAndReturnObject_SameInstance()
    {
      // Arrange
      var pool = new ConcurrentObjectPool<List<int>>(new ListPolicy());

      var list1 = pool.Get();
      pool.Return(list1);

      // Act
      var list2 = pool.Get();

      // Assert
      Assert.Same(list1, list2);
    }

    [Fact]
    public void ConcurrentObjectPool_CreatedByPolicy()
    {
      // Arrange
      var pool = new ConcurrentObjectPool<List<int>>(new ListPolicy());

      // Act
      var list = pool.Get();

      // Assert
      Assert.Equal(17, list.Capacity);
    }

    [Fact]
    public void ConcurrentObjectPool_Return_RejectedByPolicy()
    {
      // Arrange
      var pool = new ConcurrentObjectPool<List<int>>(new ListPolicy());
      var list1 = pool.Get();
      list1.Capacity = 20;

      // Act
      pool.Return(list1);
      var list2 = pool.Get();

      // Assert
      Assert.NotSame(list1, list2);
    }

    [Fact]
    public void DefaultObjectPool_GetAndReturnObject_SameInstance()
    {
      // Arrange
      var pool = new DefaultObjectPool<List<int>>(new ListPolicy());

      var list1 = pool.Get();
      pool.Return(list1);

      // Act
      var list2 = pool.Get();

      // Assert
      Assert.Same(list1, list2);
    }

    [Fact]
    public void DefaultObjectPool_CreatedByPolicy()
    {
      // Arrange
      var pool = new DefaultObjectPool<List<int>>(new ListPolicy());

      // Act
      var list = pool.Get();

      // Assert
      Assert.Equal(17, list.Capacity);
    }

    [Fact]
    public void DefaultObjectPool_Return_RejectedByPolicy()
    {
      // Arrange
      var pool = new DefaultObjectPool<List<int>>(new ListPolicy());
      var list1 = pool.Get();
      list1.Capacity = 20;

      // Act
      pool.Return(list1);
      var list2 = pool.Get();

      // Assert
      Assert.NotSame(list1, list2);
    }

    [Fact]
    public void SynchronizedObjectPool_GetAndReturnObject_SameInstance()
    {
      // Arrange
      var pool = new SynchronizedObjectPool<List<int>>(new ListPolicy());

      var list1 = pool.Get();
      pool.Return(list1);

      // Act
      var list2 = pool.Get();

      // Assert
      Assert.Same(list1, list2);
    }

    [Fact]
    public void SynchronizedObjectPool_CreatedByPolicy()
    {
      // Arrange
      var pool = new SynchronizedObjectPool<List<int>>(new ListPolicy());

      // Act
      var list = pool.Get();

      // Assert
      Assert.Equal(17, list.Capacity);
    }

    [Fact]
    public void SynchronizedObjectPool_Return_RejectedByPolicy()
    {
      // Arrange
      var pool = new SynchronizedObjectPool<List<int>>(new ListPolicy());
      var list1 = pool.Get();
      list1.Capacity = 20;

      // Act
      pool.Return(list1);
      var list2 = pool.Get();

      // Assert
      Assert.NotSame(list1, list2);
    }

    private class ListPolicy : IPooledObjectPolicy<List<int>>
    {
      public List<int> Create()
      {
        return new List<int>(17);
      }

      public List<int> PreGetting(List<int> obj)
      {
        return obj;
      }

      public bool Return(List<int> obj)
      {
        return obj.Capacity == 17;
      }
    }
  }
}
