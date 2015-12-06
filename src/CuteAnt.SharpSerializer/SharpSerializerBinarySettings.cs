#region Copyright © 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

//  ***********************************************************************
//  Project: sharpSerializer
//  Web: http://www.sharpserializer.com
//
//  This software is provided 'as-is', without any express or implied warranty.
//  In no event will the author(s) be held liable for any damages arising from
//  the use of this software.
//
//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it
//  freely, subject to the following restrictions:
//
//      1. The origin of this software must not be misrepresented; you must not
//        claim that you wrote the original software. If you use this software
//        in a product, an acknowledgment in the product documentation would be
//        appreciated but is not required.
//
//      2. Altered source versions must be plainly marked as such, and must not
//        be misrepresented as being the original software.
//
//      3. This notice may not be removed or altered from any source distribution.
//
//  ***********************************************************************

#endregion

using System.Text;
using CuteAnt.Serialization.Core;

namespace CuteAnt.Serialization
{
	/// <summary>All the most important settings for binary serialization</summary>
	/// <remarks></remarks>
	public sealed class SharpSerializerBinarySettings : SharpSerializerSettings<AdvancedSharpSerializerBinarySettings>
	{
		#region -- 属性 --

		private Encoding _Encoding = Encoding.UTF8;

		/// <summary>How are strings serialized.</summary>
		/// <value>
		/// <para></para>
		/// </value>
		/// <remarks>Default is UTF-8.</remarks>
		public Encoding Encoding
		{
			get { return _Encoding; }
			set { _Encoding = value; }
		}

		private BinarySerializationMode _Mode = BinarySerializationMode.SizeOptimized;

		/// <summary>Default is SizeOptimized - Types and property names are stored in a header.</summary>
		/// <value>
		/// <para></para>
		/// </value>
		/// <remarks>The opposite is Burst mode when all types are serialized with their objects.</remarks>
		public BinarySerializationMode Mode
		{
			get { return _Mode; }
			set { _Mode = value; }
		}

		#endregion

		#region -- 构造 --

		/// <summary>Default constructor. Serialization in SizeOptimized mode. For other modes choose an overloaded constructor</summary>
		public SharpSerializerBinarySettings()
		{
		}

		/// <summary>Overloaded constructor. Chooses mode in which the data is serialized.</summary>
		/// <param name="mode" type="CuteAnt.Serialization.BinarySerializationMode">
		/// <para>SizeOptimized - all types are stored in a header, objects only reference these types (better for collections). </para>
		/// <para>Burst - all types are serialized with their objects (better for serializing of single objects).</para>
		/// </param>
		/// <param name="encoding" type="System.Text.Encoding">
		/// <para></para>
		/// </param>
		public SharpSerializerBinarySettings(BinarySerializationMode mode, Encoding encoding = null)
		{
			if (encoding != null)
			{
				_Encoding = encoding;
			}
			_Mode = mode;
		}

		#endregion

		#region -- 实例 --

		/// <summary>创建一个新的实例</summary>
		/// <returns>A CuteAnt.Serialization.SharpSerializerBinarySettings value...</returns>
		public static SharpSerializerBinarySettings Create()
		{
			return new SharpSerializerBinarySettings();
		}

		/// <summary>创建一个新的实例</summary>
		/// <param name="mode" type="CuteAnt.Serialization.BinarySerializationMode">
		/// <para></para>
		/// </param>
		/// <param name="encoding" type="System.Text.Encoding">
		/// <para></para>
		/// </param>
		/// <returns>A CuteAnt.Serialization.SharpSerializerBinarySettings value...</returns>
		public static SharpSerializerBinarySettings Create(BinarySerializationMode mode, Encoding encoding = null)
		{
			return new SharpSerializerBinarySettings(mode, encoding);
		}

		#endregion
	}
}