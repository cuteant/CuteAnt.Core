# Microsoft.IO.RecyclableMemoryStream [![NuGet Version](https://img.shields.io/nuget/v/Microsoft.IO.RecyclableMemoryStream.svg?style=flat)](https://www.nuget.org/packages/Microsoft.IO.RecyclableMemoryStream/) 

A library to provide pooling for .NET MemoryStream objects to improve application performance. 

## Get Started

Install the latest version from NuGet (for .NET 4.5 and up)

```
Install-Package Microsoft.IO.RecyclableMemoryStream
```

## Features

- The semantics are close to the original System.IO.MemoryStream implementation, and is intended to be a drop-in replacement.
- Rather than pooling the streams themselves, the underlying buffers are pooled. This allows you to use the simple Dispose pattern to release the buffers back to the pool, as well as detect invalid usage patterns (such as reusing a stream after it’s been disposed).
- The MemoryManager is thread-safe (streams themselves are inherently NOT thread safe).
- Each stream can be tagged with an identifying string that is used in logging - helpful when finding bugs and memory leaks relating to incorrect pool use.
- Debug features like recording the call stack of the stream allocation to track down pool leaks
- Maximum free pool size to handle spikes in usage without using too much memory.
- Flexible and adjustable limits to the pooling algorithm.
- Metrics tracking and events so that you can see the impact on the system.

For more details, refer to the [announcement blog post](http://www.philosophicalgeek.com/2015/02/06/announcing-microsoft-io-recycablememorystream/)

## Build Targets

The code ships utilizing CommonBuildToolset. This provides some build extensions over top of MSBuild to automatically pull
NuGet packages from command line build, etc.

MSBuild 15 is required to build the code. You get this with Visual Studio 2017. Cross-platform builds will be documented
in a future release.

## Testing

We're using Visual Studio 2017 for interactive test work. Requirements:
- NUnit test adaptor (VS Extension)
- Be sure to set the default processor architecture for tests to x64 (or giant allocation test will fail)
