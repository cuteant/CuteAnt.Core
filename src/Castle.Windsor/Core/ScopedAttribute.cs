// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Core
{
	using System;

	/// <summary>
	///   Indicates that the target components wants a
	///   per thread lifestyle.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ScopedAttribute : LifestyleAttribute
	{
		/// <summary>
		///   Initializes a new instance of the <see cref = "PerThreadAttribute" /> class.
		/// </summary>
		public ScopedAttribute()
			: base(LifestyleType.Scoped)
		{
		}

		public Type ScopeAccessorType { get; set; }
	}
}