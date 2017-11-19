﻿using System;

namespace IocPerformance.Classes.Complex
{
    public interface IComplex1
    {
    }

    public interface IComplex2
    {
    }

    public interface IComplex3
    {
    }

    public class Complex1 : IComplex1
    {
        private static int counter;

        public Complex1(
            IFirstService firstService,
            ISecondService secondService,
            IThirdService thirdService,
            ISubObjectOne subObjectOne,
            ISubObjectTwo subObjectTwo,
            ISubObjectThree subObjectThree)
        {
            if (firstService == null)
            {
                throw new ArgumentNullException(nameof(firstService));
            }

            if (secondService == null)
            {
                throw new ArgumentNullException(nameof(secondService));
            }

            if (thirdService == null)
            {
                throw new ArgumentNullException(nameof(thirdService));
            }

            if (subObjectOne == null)
            {
                throw new ArgumentNullException(nameof(subObjectOne));
            }

            if (subObjectTwo == null)
            {
                throw new ArgumentNullException(nameof(subObjectTwo));
            }

            if (subObjectThree == null)
            {
                throw new ArgumentNullException(nameof(subObjectThree));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }

    public class Complex2 : IComplex2
    {
        private static int counter;

        public Complex2(
            IFirstService firstService,
            ISecondService secondService,
            IThirdService thirdService,
            ISubObjectOne subObjectOne,
            ISubObjectTwo subObjectTwo,
            ISubObjectThree subObjectThree)
        {
            if (firstService == null)
            {
                throw new ArgumentNullException(nameof(firstService));
            }

            if (secondService == null)
            {
                throw new ArgumentNullException(nameof(secondService));
            }

            if (thirdService == null)
            {
                throw new ArgumentNullException(nameof(thirdService));
            }

            if (subObjectOne == null)
            {
                throw new ArgumentNullException(nameof(subObjectOne));
            }

            if (subObjectTwo == null)
            {
                throw new ArgumentNullException(nameof(subObjectTwo));
            }

            if (subObjectThree == null)
            {
                throw new ArgumentNullException(nameof(subObjectThree));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }

    public class Complex3 : IComplex3
    {
        private static int counter;

        public Complex3(
            IFirstService firstService,
            ISecondService secondService,
            IThirdService thirdService,
            ISubObjectOne subObjectOne,
            ISubObjectTwo subObjectTwo,
            ISubObjectThree subObjectThree)
        {
            if (firstService == null)
            {
                throw new ArgumentNullException(nameof(firstService));
            }

            if (secondService == null)
            {
                throw new ArgumentNullException(nameof(secondService));
            }

            if (thirdService == null)
            {
                throw new ArgumentNullException(nameof(thirdService));
            }

            if (subObjectOne == null)
            {
                throw new ArgumentNullException(nameof(subObjectOne));
            }

            if (subObjectTwo == null)
            {
                throw new ArgumentNullException(nameof(subObjectTwo));
            }

            if (subObjectThree == null)
            {
                throw new ArgumentNullException(nameof(subObjectThree));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }
}
