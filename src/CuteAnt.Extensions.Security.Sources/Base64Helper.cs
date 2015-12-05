using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace CuteAnt.Security
{
	/// <summary>Base64编码</summary>
	internal static class Base64Helper
	{
		/// <summary>Base编码</summary>
		/// <param name="instream"></param>
		/// <param name="outstream"></param>
		internal static void Encode(Stream instream, Stream outstream)
		{
			// Create a new ToBase64Transform object to convert to base 64.
			using (var base64Transform = new ToBase64Transform())
			{
				// Create a new byte array with the size of the output block size.
				var outputBytes = new Byte[base64Transform.OutputBlockSize];

				// Retrieve the file contents into a byte array.
				//var inputBytes = new Byte[instream.Length];
				//instream.Read(inputBytes, 0, inputBytes.Length);
				var inputBytes = instream.ReadBytes();

				// Verify that multiple blocks can not be transformed.
				if (!base64Transform.CanTransformMultipleBlocks)
				{
					// Initializie the offset size.
					var inputOffset = 0;

					// Iterate through inputBytes transforming by blockSize.
					var inputBlockSize = base64Transform.InputBlockSize;

					while (inputBytes.Length - inputOffset > inputBlockSize)
					{
						base64Transform.TransformBlock(inputBytes, inputOffset, inputBytes.Length - inputOffset, outputBytes, 0);

						inputOffset += base64Transform.InputBlockSize;
						outstream.Write(outputBytes, 0, base64Transform.OutputBlockSize);
					}

					// Transform the final block of data.
					outputBytes = base64Transform.TransformFinalBlock(inputBytes, inputOffset, inputBytes.Length - inputOffset);

					outstream.Write(outputBytes, 0, outputBytes.Length);
				}

				// Determine if the current transform can be reused.
				if (!base64Transform.CanReuseTransform)
				{
					// Free up any used resources.
					base64Transform.Clear();
				}
			}
		}

		/// <summary>Base解码</summary>
		/// <param name="instream"></param>
		/// <param name="outstream"></param>
		internal static void Decode(Stream instream, Stream outstream)
		{
			using (var base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces))
			{
				var outputBytes = new Byte[base64Transform.OutputBlockSize];

				// Retrieve the file contents into a byte array. 
				//var inputBytes = new Byte[instream.Length];
				//instream.Read(inputBytes, 0, inputBytes.Length);
				var inputBytes = instream.ReadBytes();

				// Transform the data in chunks the size of InputBlockSize. 
				int i = 0;
				while (inputBytes.Length - i > 4/*myTransform.InputBlockSize*/)
				{
					int bytesWritten = base64Transform.TransformBlock(inputBytes, i, 4/*myTransform.InputBlockSize*/, outputBytes, 0);
					i += 4/*myTransform.InputBlockSize*/;
					outstream.Write(outputBytes, 0, bytesWritten);
				}

				// Transform the final block of data.
				outputBytes = base64Transform.TransformFinalBlock(inputBytes, i, inputBytes.Length - i);
				outstream.Write(outputBytes, 0, outputBytes.Length);

				// Free up any used resources.
				base64Transform.Clear();
			}
		}
	}
}
