﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Collections.Generic;
using CuteAnt.IO.Pipelines.Testing;
using System.Linq;
using System.Text;
using CuteAnt;

namespace CuteAnt.IO.Pipelines.Tests
{
    public abstract class ReadOnlyBufferFactory
    {
        public static ReadOnlyBufferFactory Array { get; } = new ArrayTestBufferFactory();
        public static ReadOnlyBufferFactory OwnedMemory { get; } = new OwnedMemoryTestBufferFactory();
        public static ReadOnlyBufferFactory SingleSegment { get; } = new SingleSegmentTestBufferFactory();
        public static ReadOnlyBufferFactory SegmentPerByte { get; } = new BytePerSegmentTestBufferFactory();

        public abstract ReadOnlyBuffer<byte> CreateOfSize(int size);
        public abstract ReadOnlyBuffer<byte> CreateWithContent(byte[] data);

        public ReadOnlyBuffer<byte> CreateWithContent(string data)
        {
            return CreateWithContent(Encoding.ASCII.GetBytes(data));
        }
        
        internal class ArrayTestBufferFactory : ReadOnlyBufferFactory
        {
            public override ReadOnlyBuffer<byte> CreateOfSize(int size)
            {
                return new ReadOnlyBuffer<byte>(new byte[size + 20], 10, size);
            }

            public override ReadOnlyBuffer<byte> CreateWithContent(byte[] data)
            {
                var startSegment = new byte[data.Length + 20];
                System.Array.Copy(data, 0, startSegment, 10, data.Length);
                return new ReadOnlyBuffer<byte>(startSegment, 10, data.Length);
            }
        }

        internal class OwnedMemoryTestBufferFactory : ReadOnlyBufferFactory
        {
            public override ReadOnlyBuffer<byte> CreateOfSize(int size)
            {
                return new ReadOnlyBuffer<byte>(new OwnedArray<byte>(size + 20), 10, size);
            }

            public override ReadOnlyBuffer<byte> CreateWithContent(byte[] data)
            {
                var startSegment = new byte[data.Length + 20];
                System.Array.Copy(data, 0, startSegment, 10, data.Length);
                return new ReadOnlyBuffer<byte>(new OwnedArray<byte>(startSegment), 10, data.Length);
            }
        }
 
        internal class SingleSegmentTestBufferFactory: ReadOnlyBufferFactory
        {
            public override ReadOnlyBuffer<byte> CreateOfSize(int size)
            {
                return BufferUtilities.CreateBuffer(size);
            }

            public override ReadOnlyBuffer<byte> CreateWithContent(byte[] data)
            {
                return BufferUtilities.CreateBuffer(data);
            }
        }

        internal class BytePerSegmentTestBufferFactory: ReadOnlyBufferFactory
        {
            public override ReadOnlyBuffer<byte> CreateOfSize(int size)
            {
                return CreateWithContent(new byte[size]);
            }

            public override ReadOnlyBuffer<byte> CreateWithContent(byte[] data)
            {
                var segments = new List<byte[]>();

                segments.Add(EmptyArray<byte>.Instance);
                foreach (var b in data)
                {
                    segments.Add(new [] { b });
                    segments.Add(EmptyArray<byte>.Instance);
                }

                return BufferUtilities.CreateBuffer(segments.ToArray());
            }
        }
    }
}
