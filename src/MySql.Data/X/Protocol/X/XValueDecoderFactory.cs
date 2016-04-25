﻿// Copyright © 2015, 2016 Oracle and/or its affiliates. All rights reserved.
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


using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

namespace MySqlX.Protocol.X
{
  internal class XValueDecoderFactory
  {
    public static ValueDecoder GetValueDecoder(Column c, Mysqlx.Resultset.ColumnMetaData.Types.FieldType type)
    {
      switch (type)
      {
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.BIT: return new BitDecoder();
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.BYTES: return new ByteDecoder(false);
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.ENUM: return new ByteDecoder(true);
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.SET: return new SetDecoder();
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.TIME: return new XTimeDecoder();
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.DATETIME: return new XDateTimeDecoder();
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.SINT: return new IntegerDecoder(true);
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.UINT: return new IntegerDecoder(false);
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.FLOAT: return new FloatDecoder(true);
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.DOUBLE: return new FloatDecoder(false);
        case Mysqlx.Resultset.ColumnMetaData.Types.FieldType.DECIMAL: return new DecimalDecoder();
      }
      throw new MySqlException("Unknown field type " + type.ToString());
    }
  }
}
