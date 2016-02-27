﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.Extensions.Primitives;

namespace CuteAnt.Extensions.FileProviders.Physical
{
  public class FileChangeToken : IChangeToken
  {
    private Regex _searchRegex;

    public FileChangeToken(string pattern)
    {
      Pattern = pattern;
    }

    public string Pattern { get; }

    private CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

    private Regex SearchRegex
    {
      get
      {
        if (_searchRegex == null)
        {
          _searchRegex = new Regex(
              '^' + Pattern + '$',
              RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
#if !NET40 // ## 苦竹 修改 ##
              ,Constants.RegexMatchTimeout
#endif
              );
        }

        return _searchRegex;
      }
    }

    public bool ActiveChangeCallbacks => true;

    public bool HasChanged => TokenSource.Token.IsCancellationRequested;

    public IDisposable RegisterChangeCallback(Action<object> callback, object state)
    {
      return TokenSource.Token.Register(callback, state);
    }

    public bool IsMatch(string relativePath)
    {
      return SearchRegex.IsMatch(relativePath);
    }

    public void Changed()
    {
#if NET40
      TaskEx
#else
      Task
#endif
        .Run(() =>
      {
        try
        {
          TokenSource.Cancel();
        }
        catch
        {
        }
      });
    }
  }
}