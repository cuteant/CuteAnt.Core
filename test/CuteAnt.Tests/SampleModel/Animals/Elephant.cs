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

using System;
using CuteAnt.SampleModel.Animals.Enumerations;

namespace CuteAnt.SampleModel.Animals
{
  internal interface IElephant
  {
    void Roar(int count);
    void Roar(int count, int volume);
    void Accept(char c);
    void AcceptParams(params string[] args);
  }

  internal class Elephant : Mammal, IElephant
  {
#pragma warning disable 0169, 0649
    public int MethodInvoked { get; private set; }
#pragma warning restore 0169, 0649

    #region Constructors
    public Elephant() : base(Climate.Hot, MovementCapabilities.Land)
    {
    }
    #endregion

    internal static void MakeInternal(Elephant obj)
    {
      obj.MethodInvoked = 100;
    }

    #region Methods
    public void Eat()
    {
      MethodInvoked = 1;
    }
    public void Eat(string food)
    {
      MethodInvoked = 2;
    }
    public void Eat(int count)
    {
      MethodInvoked = 3;
    }
    public void Eat(int count, string food)
    {
      MethodInvoked = 4;
    }
    public void Eat(double count, string food, bool isHay)
    {
      MethodInvoked = 5;
    }
    public string Name;
    public void Eat(bool isHay, double count, string food, string name = "a", int age = 168)
    {
      MethodInvoked = age;
      Name = name;
    }

    public void Roar(int count)
    {
      MethodInvoked = 10;
    }
    public void Roar(int count, int volume)
    {
      MethodInvoked = 11;
    }
    public void Accept(char c)
    {
      MethodInvoked = 12;
    }
    public void AcceptParams(params string[] args)
    {
      MethodInvoked = 13;
    }
    #endregion
  }
}
