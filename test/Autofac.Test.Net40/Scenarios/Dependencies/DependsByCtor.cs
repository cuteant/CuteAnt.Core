﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Dependencies
{
    public class DependsByCtor
    {
        public DependsByCtor(DependsByProp o)
        {
            Dep = o;
        }

        public DependsByProp Dep { get; private set; }
    }
}
