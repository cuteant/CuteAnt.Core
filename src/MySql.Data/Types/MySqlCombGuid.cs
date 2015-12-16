// Copyright (c) 2004-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Properties;
using CuteAnt;

namespace MySql.Data.Types
{
	internal struct MySqlCombGuid : IMySqlValue
	{
		CombGuid mValue;
		private bool isNull;
		private byte[] bytes;
		private bool _OldGuids;

		public MySqlCombGuid(byte[] buff, bool oldGuids)
		{
			_OldGuids = oldGuids;
			if (!_OldGuids)
			{
				mValue = new CombGuid(buff, CombGuidSequentialSegmentType.Comb);
			}
			else
			{
				mValue = new CombGuid(buff, CombGuidSequentialSegmentType.Guid);
			}
			isNull = false;
			bytes = buff;
		}

		public byte[] Bytes
		{
			get { return bytes; }
		}

		public bool OldGuids
		{
			get { return _OldGuids; }
			set { _OldGuids = value; }
		}

		#region IMySqlValue Members

		public bool IsNull
		{
			get { return isNull; }
		}

		MySqlDbType IMySqlValue.MySqlDbType
		{
			get { return MySqlDbType.CombGuid; }
		}

		object IMySqlValue.Value
		{
			get { return mValue; }
		}

		public CombGuid Value
		{
			get { return mValue; }
		}

		Type IMySqlValue.SystemType
		{
			get { return typeof(CombGuid); }
		}

		string IMySqlValue.MySqlTypeName
		{
			get { return "BINARY(16)"; }
		}

		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			CombGuid guid = CombGuid.Empty;
			string valAsString = val as string;
			byte[] valAsByte = val as byte[];

			if (val.GetType() == typeof(CombGuid))
			{
				guid = (CombGuid)val;
			}
			else if (val.GetType() == typeof(Guid))
			{
				guid = (Guid)val;
			}
			else
			{
				try
				{
					if (valAsString != null)
						guid = !OldGuids ? new CombGuid(valAsString, CombGuidSequentialSegmentType.Comb) : new CombGuid(valAsString, CombGuidSequentialSegmentType.Guid);
					else if (valAsByte != null)
						guid = !OldGuids ? new CombGuid(valAsByte, CombGuidSequentialSegmentType.Comb) : new CombGuid(valAsByte, CombGuidSequentialSegmentType.Guid);
				}
				catch (Exception ex)
				{
					throw new MySqlException(Resources.DataNotInSupportedFormat, ex);
				}
			}

			byte[] bytes = !OldGuids ? guid.GetByteArray(CombGuidSequentialSegmentType.Comb) : guid.GetByteArray(CombGuidSequentialSegmentType.Guid);

			if (binary)
			{
				packet.WriteLength(bytes.Length);
				packet.Write(bytes);
			}
			else
			{
				packet.WriteStringNoNull("_binary ");
				packet.WriteByte((byte)'\'');
				EscapeByteArray(bytes, bytes.Length, packet);
				packet.WriteByte((byte)'\'');
			}
		}

		private static void EscapeByteArray(byte[] bytes, int length, MySqlPacket packet)
		{
			for (int x = 0; x < length; x++)
			{
				byte b = bytes[x];
				if (b == '\0')
				{
					packet.WriteByte((byte)'\\');
					packet.WriteByte((byte)'0');
				}

				else if (b == '\\' || b == '\'' || b == '\"')
				{
					packet.WriteByte((byte)'\\');
					packet.WriteByte(b);
				}
				else
					packet.WriteByte(b);
			}
		}

		private MySqlCombGuid ReadOldGuid(MySqlPacket packet, long length)
		{
			if (length == -1)
				length = (long)packet.ReadFieldLength();

			byte[] buff = new byte[length];
			packet.Read(buff, 0, (int)length);
			MySqlCombGuid g = new MySqlCombGuid(buff, OldGuids);
			return g;
		}

		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (!nullVal)
			{
				return ReadOldGuid(packet, length);
			}
			else
			{
				MySqlCombGuid g = new MySqlCombGuid();
				g.isNull = true;
				g.OldGuids = OldGuids;
				return g;
			}
		}

		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			int len = (int)packet.ReadFieldLength();
			packet.Position += len;
		}

		#endregion

		public static void SetDSInfo(MySqlSchemaCollection sc)
		{
			// we use name indexing because this method will only be called
			// when GetSchema is called for the DataSourceInformation 
			// collection and then it wil be cached.
			MySqlSchemaRow row = sc.AddRow();
			row["TypeName"] = "COMBGUID";
			row["ProviderDbType"] = MySqlDbType.CombGuid;
			row["ColumnSize"] = 0;
			row["CreateFormat"] = "BINARY(16)";
			row["CreateParameters"] = null;
			row["DataType"] = "CuteAnt.CombGuid";
			row["IsAutoincrementable"] = false;
			row["IsBestMatch"] = true;
			row["IsCaseSensitive"] = false;
			row["IsFixedLength"] = true;
			row["IsFixedPrecisionScale"] = true;
			row["IsLong"] = false;
			row["IsNullable"] = true;
			row["IsSearchable"] = false;
			row["IsSearchableWithLike"] = false;
			row["IsUnsigned"] = false;
			row["MaximumScale"] = 0;
			row["MinimumScale"] = 0;
			row["IsConcurrencyType"] = DBNull.Value;
			row["IsLiteralSupported"] = false;
			row["LiteralPrefix"] = null;
			row["LiteralSuffix"] = null;
			row["NativeDataType"] = null;
		}
	}
}
