﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Dependencies.Circularity
{
    public class A : IA
    {
        public A(IC c)
        {
        }
    }
}
