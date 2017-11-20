﻿#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion

using CuteAnt.SampleModel.Animals.Attributes;
using CuteAnt.SampleModel.Animals.Enumerations;
using CuteAnt.SampleModel.Animals.Interfaces;

namespace CuteAnt.SampleModel.Animals
{
	[Zone(Zone.Savannah)]
	internal class Giraffe : Mammal, ISwim
	{
		public Giraffe( int id, Climate climateRequirements, MovementCapabilities movementCapabilities ) : base( id, climateRequirements, movementCapabilities )
		{
		}

		public Giraffe( Climate climateRequirements, MovementCapabilities movementCapabilities ) : base( climateRequirements, movementCapabilities )
		{
		}

		#region ISwim Members
		double ISwim.SwimDistance
		{
			get { throw new System.NotImplementedException(); }
		}

		void ISwim.Move( double distance )
		{
			throw new System.NotImplementedException();
		}
		#endregion
	}
}
