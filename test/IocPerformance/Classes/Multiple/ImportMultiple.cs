using System;
using System.Collections.Generic;

namespace IocPerformance.Classes.Multiple
{
    public class ImportMultiple1
    {
        private static int counter;

        public ImportMultiple1(
            IEnumerable<ISimpleAdapter> adapters)
        {
            if (adapters == null)
            {
                throw new ArgumentNullException(nameof(adapters));
            }

            int adapterCount = 0;
            foreach (var adapter in adapters)
            {
                if (adapter == null)
                {
                    throw new ArgumentException("adapters item should be not null");
                }

                ++adapterCount;
            }

            if (adapterCount != 5)
            {
                throw new ArgumentException("there should be 5 adapters and there where: " + adapterCount, nameof(adapters));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }

    public class ImportMultiple2
    {
        private static int counter;

        public ImportMultiple2(
            IEnumerable<ISimpleAdapter> adapters)
        {
            if (adapters == null)
            {
                throw new ArgumentNullException(nameof(adapters));
            }

            int adapterCount = 0;
            foreach (var adapter in adapters)
            {
                if (adapter == null)
                {
                    throw new ArgumentException("adapters item should be not null");
                }

                ++adapterCount;
            }

            if (adapterCount != 5)
            {
                throw new ArgumentException("there should be 5 adapters and there where: " + adapterCount, nameof(adapters));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }

    public class ImportMultiple3
    {
        private static int counter;

        public ImportMultiple3(
            IEnumerable<ISimpleAdapter> adapters)
        {
            if (adapters == null)
            {
                throw new ArgumentNullException(nameof(adapters));
            }

            int adapterCount = 0;
            foreach (var adapter in adapters)
            {
                if (adapter == null)
                {
                    throw new ArgumentException("adapters item should be not null");
                }

                ++adapterCount;
            }

            if (adapterCount != 5)
            {
                throw new ArgumentException("there should be 5 adapters and there where: " + adapterCount, nameof(adapters));
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
