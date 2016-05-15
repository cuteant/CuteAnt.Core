﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.Extensions.FileSystemGlobbing;
using CuteAnt.Extensions.FileSystemGlobbing.Abstractions;
using CuteAnt.Extensions.Primitives;

namespace CuteAnt.Extensions.FileProviders.Physical
{
  public class PhysicalFilesWatcher : IDisposable
  {
    private readonly ConcurrentDictionary<string, ChangeTokenInfo> _matchInfoCache =
        new ConcurrentDictionary<string, ChangeTokenInfo>(StringComparer.OrdinalIgnoreCase);
    private readonly FileSystemWatcher _fileWatcher;
    private readonly object _lockObject = new object();
    private readonly string _root;
    private readonly bool _pollForChanges;

    public PhysicalFilesWatcher(
        string root,
        FileSystemWatcher fileSystemWatcher,
        bool pollForChanges)
    {
      _root = root;
      _fileWatcher = fileSystemWatcher;
      _fileWatcher.IncludeSubdirectories = true;
      _fileWatcher.Created += OnChanged;
      _fileWatcher.Changed += OnChanged;
      _fileWatcher.Renamed += OnRenamed;
      _fileWatcher.Deleted += OnChanged;
      _fileWatcher.Error += OnError;

      _pollForChanges = pollForChanges;
    }

    public IChangeToken CreateFileChangeToken(string filter)
    {
      if (filter == null)
      {
        throw new ArgumentNullException(nameof(filter));
      }

      filter = NormalizePath(filter);

      IChangeToken changeToken;
      var isWildCard = filter.IndexOf('*') != -1;
      if (isWildCard || IsDirectoryPath(filter))
      {
        changeToken = ResolveFileTokensForGlobbingPattern(filter);
      }
      else
      {
        changeToken = GetOrAddChangeToken(filter);
      }

      lock (_lockObject)
      {
        if (_matchInfoCache.Count > 0 && !_fileWatcher.EnableRaisingEvents)
        {
          // Perf: Turn on the file monitoring if there is something to monitor.
          _fileWatcher.EnableRaisingEvents = true;
        }
      }

      return changeToken;
    }

    private IChangeToken GetOrAddChangeToken(string filePath)
    {
      ChangeTokenInfo tokenInfo;
      if (!_matchInfoCache.TryGetValue(filePath, out tokenInfo))
      {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
        tokenInfo = new ChangeTokenInfo(cancellationTokenSource, cancellationChangeToken);
        tokenInfo = _matchInfoCache.GetOrAdd(filePath, tokenInfo);
      }

      IChangeToken changeToken = tokenInfo.ChangeToken;
      if (_pollForChanges)
      {
        // The expiry of CancellationChangeToken is controlled by this type and consequently we can cache it.
        // PollingFileChangeToken on the other hand manages its own lifetime and consequently we cannot cache it.
        changeToken = new CompositeFileChangeToken(
            new[]
            {
                        changeToken,
                        new PollingFileChangeToken(new FileInfo(filePath))
            });
      }

      return changeToken;
    }

    private IChangeToken ResolveFileTokensForGlobbingPattern(string filter)
    {
      var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
      matcher.AddInclude(filter);

      var directoryBase = new DirectoryInfoWrapper(new DirectoryInfo(_root));
      var result = matcher.Execute(directoryBase);

      var changeTokens = new List<IChangeToken>();
      foreach (var file in result.Files)
      {
        var changeToken = GetOrAddChangeToken(file.Path);
        changeTokens.Add(changeToken);
      }

      return new CompositeFileChangeToken(changeTokens);
    }

    public void Dispose()
    {
      _fileWatcher.Dispose();
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
      // For a file name change or a directory's name change notify registered tokens.
      OnFileSystemEntryChange(e.OldFullPath);
      OnFileSystemEntryChange(e.FullPath);

      if (Directory.Exists(e.FullPath))
      {
        try
        {
          // If the renamed entity is a directory then notify tokens for every sub item.
          foreach (var newLocation in Directory.EnumerateFileSystemEntries(e.FullPath, "*", SearchOption.AllDirectories))
          {
            // Calculated previous path of this moved item.
            var oldLocation = Path.Combine(e.OldFullPath, newLocation.Substring(e.FullPath.Length + 1));
            OnFileSystemEntryChange(oldLocation);
            OnFileSystemEntryChange(newLocation);
          }
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is SecurityException ||
            ex is DirectoryNotFoundException ||
            ex is UnauthorizedAccessException)
        {
          // Swallow the exception.
        }
      }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
      OnFileSystemEntryChange(e.FullPath);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
      // Notify all cache entries on error.
      foreach (var path in _matchInfoCache.Keys)
      {
        ReportChangeForMatchedEntries(path);
      }
    }

    private void OnFileSystemEntryChange(string fullPath)
    {
      var fileSystemInfo = new FileInfo(fullPath);
      if (FileSystemInfoHelper.IsHiddenFile(fileSystemInfo))
      {
        return;
      }

      var relativePath = fullPath.Substring(_root.Length);
      ReportChangeForMatchedEntries(relativePath);
    }

    private void ReportChangeForMatchedEntries(string path)
    {
      path = NormalizePath(path);

      ChangeTokenInfo matchInfo;
      if (_matchInfoCache.TryRemove(path, out matchInfo))
      {
        CancelToken(matchInfo);

        if (_matchInfoCache.Count == 0)
        {
          lock (_lockObject)
          {
            if (_matchInfoCache.Count == 0 && _fileWatcher.EnableRaisingEvents)
            {
              // Perf: Turn off the file monitoring if no files to monitor.
              _fileWatcher.EnableRaisingEvents = false;
            }
          }
        }
      }
    }

    private static string NormalizePath(string filter) => filter = filter.Replace('\\', '/');

    private static bool IsDirectoryPath(string path)
    {
      return path.Length > 0
          && (path[path.Length - 1] == Path.DirectorySeparatorChar || path[path.Length - 1] == Path.AltDirectorySeparatorChar);
    }

    private static void CancelToken(ChangeTokenInfo matchInfo)
    {
      if (matchInfo.TokenSource.IsCancellationRequested)
      {
        return;
      }

#if NET40
      TaskEx.Run(() =>
#else
      Task.Run(() =>
#endif
      {
        try
        {
          matchInfo.TokenSource.Cancel();
        }
        catch
        {

        }
      });
    }

    private struct ChangeTokenInfo
    {
      public ChangeTokenInfo(
          CancellationTokenSource tokenSource,
          CancellationChangeToken changeToken)
      {
        TokenSource = tokenSource;
        ChangeToken = changeToken;
      }

      public CancellationTokenSource TokenSource { get; }

      public CancellationChangeToken ChangeToken { get; }
    }
  }
}