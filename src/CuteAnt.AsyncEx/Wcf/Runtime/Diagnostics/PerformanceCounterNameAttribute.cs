//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace CuteAnt.Diagnostics
{
    using System;
    
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    sealed class PerformanceCounterNameAttribute : Attribute
    {          
        public PerformanceCounterNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            set;
        }
    }
}
