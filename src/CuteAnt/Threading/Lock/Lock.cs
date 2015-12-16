// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace CuteAnt.Threading
{
	using System;

	public abstract class Lock : DisposeBase
	{
		public abstract IUpgradeableLockHolder ForReadingUpgradeable();

		public abstract ILockHolder ForReading();

		public abstract ILockHolder ForWriting();

		public abstract IUpgradeableLockHolder ForReadingUpgradeable(Boolean waitForLock);

		public abstract ILockHolder ForReading(Boolean waitForLock);

		public abstract ILockHolder ForWriting(Boolean waitForLock);

		/// <summary>Creates a new lock.</summary>
		/// <returns></returns>
		public static Lock Create()
		{
			return new SlimReadWriteLock();
		}
	}
}