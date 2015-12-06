#region License

//
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

using System;
using System.Collections.Generic;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class IndexColumnDefinition : IEquatable<IndexColumnDefinition>
	{
		internal virtual String Name { get; set; }

		internal virtual Boolean IsDescending { get; set; }

		internal IndexColumnDefinition(String columnName, Boolean isDescending = false)
		{
			Name = columnName;
			IsDescending = isDescending;
		}

		/// <summary>重写一下</summary>
		/// <returns></returns>
		public override Int32 GetHashCode()
		{
			return Name.GetHashCode();
		}

		/// <summary>重写一下</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override Boolean Equals(object obj)
		{
			var column = obj as IndexColumnDefinition;
			return Equals(column);
		}

		/// <summary>相等</summary>
		/// <param name="column"></param>
		/// <returns></returns>
		public Boolean Equals(IndexColumnDefinition column)
		{
			if (column == null) { return false; }

			return Name == column.Name;
		}
	}
}