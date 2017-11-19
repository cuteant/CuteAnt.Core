using System;

namespace IocPerformance.Classes.Properties
{
    public interface IComplexPropertyObject1
    {
        void Verify(string containerName);
    }

    public interface IComplexPropertyObject2
    {
        void Verify(string containerName);
    }

    public interface IComplexPropertyObject3
    {
        void Verify(string containerName);
    }

    public class ComplexPropertyObject1 : IComplexPropertyObject1
    {
        private static int counter;

        public ComplexPropertyObject1()
        {
            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public IServiceA ServiceA { get; set; }

        public IServiceB ServiceB { get; set; }

        public IServiceC ServiceC { get; set; }

        public ISubObjectA SubObjectA { get; set; }

        public ISubObjectB SubObjectB { get; set; }

        public ISubObjectC SubObjectC { get; set; }

        public void Verify(string containerName)
        {
            if (this.ServiceA == null)
            {
                throw new Exception("ServiceA is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.ServiceB == null)
            {
                throw new Exception("ServiceB is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.ServiceC == null)
            {
                throw new Exception("ServiceC is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.SubObjectA == null)
            {
                throw new Exception("SubObjectA is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectA.Verify(containerName);

            if (this.SubObjectB == null)
            {
                throw new Exception("SubObjectB is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectB.Verify(containerName);

            if (this.SubObjectC == null)
            {
                throw new Exception("SubObjectC is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectC.Verify(containerName);
        }
    }

    public class ComplexPropertyObject2 : IComplexPropertyObject2
    {
        private static int counter;

        public ComplexPropertyObject2()
        {
            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public IServiceA ServiceA { get; set; }

        public IServiceB ServiceB { get; set; }

        public IServiceC ServiceC { get; set; }

        public ISubObjectA SubObjectA { get; set; }

        public ISubObjectB SubObjectB { get; set; }

        public ISubObjectC SubObjectC { get; set; }

        public void Verify(string containerName)
        {
            if (this.ServiceA == null)
            {
                throw new Exception("ServiceA is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.ServiceB == null)
            {
                throw new Exception("ServiceB is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.ServiceC == null)
            {
                throw new Exception("ServiceC is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.SubObjectA == null)
            {
                throw new Exception("SubObjectA is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectA.Verify(containerName);

            if (this.SubObjectB == null)
            {
                throw new Exception("SubObjectB is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectB.Verify(containerName);

            if (this.SubObjectC == null)
            {
                throw new Exception("SubObjectC is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectC.Verify(containerName);
        }
    }

    public class ComplexPropertyObject3 : IComplexPropertyObject3
    {
        private static int counter;

        public ComplexPropertyObject3()
        {
            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public IServiceA ServiceA { get; set; }

        public IServiceB ServiceB { get; set; }

        public IServiceC ServiceC { get; set; }

        public ISubObjectA SubObjectA { get; set; }

        public ISubObjectB SubObjectB { get; set; }

        public ISubObjectC SubObjectC { get; set; }

        public void Verify(string containerName)
        {
            if (this.ServiceA == null)
            {
                throw new Exception("ServiceA is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.ServiceB == null)
            {
                throw new Exception("ServiceB is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.ServiceC == null)
            {
                throw new Exception("ServiceC is null on ComplexPropertyObject for container " + containerName);
            }

            if (this.SubObjectA == null)
            {
                throw new Exception("SubObjectA is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectA.Verify(containerName);

            if (this.SubObjectB == null)
            {
                throw new Exception("SubObjectB is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectB.Verify(containerName);

            if (this.SubObjectC == null)
            {
                throw new Exception("SubObjectC is null on ComplexPropertyObject for container " + containerName);
            }

            this.SubObjectC.Verify(containerName);
        }
    }
}
