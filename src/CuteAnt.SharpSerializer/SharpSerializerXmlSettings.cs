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

using System.Globalization;
using System.Text;
using CuteAnt.Serialization.Core;

namespace CuteAnt.Serialization
{
	/// <summary>All the most important settings for xml serialization</summary>
	public sealed class SharpSerializerXmlSettings : SharpSerializerSettings<AdvancedSharpSerializerXmlSettings>
	{
		#region -- 属性 --

		private CultureInfo _Culture = CultureInfo.InvariantCulture;

		/// <summary>
		/// All Single numbers and date/time values are stored as text according to the Culture. Default is CultureInfo.InvariantCulture.
		/// This setting is overridden if you set AdvancedSettings.SimpleValueConverter
		/// </summary>
		public CultureInfo Culture
		{
			get { return _Culture; }
			set { _Culture = value; }
		}

		private Encoding _Encoding = Encoding.UTF8;

		/// <summary>
		/// Describes format in which the xml file is stored.Default is UTF-8.
		/// This setting is overridden if you set AdvancedSettings.XmlWriterSettings
		/// </summary>
		public Encoding Encoding
		{
			get { return _Encoding; }
			set { _Encoding = value; }
		}

		#endregion

		#region -- 构造 --

		/// <summary>Standard constructor with Culture=InvariantCulture and Encoding=UTF8</summary>
		public SharpSerializerXmlSettings()
		{
		}

		/// <summary>构造函数</summary>
		/// <param name="encoding" type="System.Text.Encoding">
		/// <para></para>
		/// </param>
		/// <param name="culture" type="System.Globalization.CultureInfo">
		/// <para></para>
		/// </param>
		public SharpSerializerXmlSettings(Encoding encoding, CultureInfo culture = null)
		{
			if (culture != null)
			{
				_Culture = culture;
			}
			_Encoding = encoding;
		}

		#endregion

		#region -- 实例 --

		/// <summary>创建一个新的实例</summary>
		/// <returns>A CuteAnt.Serialization.SharpSerializerXmlSettings value...</returns>
		public static SharpSerializerXmlSettings Create()
		{
			return new SharpSerializerXmlSettings();
		}

		/// <summary>创建一个新的实例</summary>
		/// <param name="encoding" type="System.Text.Encoding">
		/// <para></para>
		/// </param>
		/// <param name="culture" type="System.Globalization.CultureInfo">
		/// <para></para>
		/// </param>
		/// <returns>A CuteAnt.Serialization.SharpSerializerXmlSettings value...</returns>
		public static SharpSerializerXmlSettings Create(Encoding encoding, CultureInfo culture = null)
		{
			return new SharpSerializerXmlSettings(encoding, culture);
		}

		#endregion
	}
}