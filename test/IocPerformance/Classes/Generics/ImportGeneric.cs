using System;

namespace IocPerformance.Classes.Generics
{
    public class ImportGeneric<T>
    {
        private static int counter;

        public ImportGeneric(IGenericInterface<T> importGenericInterface)
        {
            if (importGenericInterface == null)
            {
                throw new ArgumentNullException(nameof(importGenericInterface));
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
