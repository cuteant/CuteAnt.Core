using System;

namespace IocPerformance.Classes.Properties
{
    public interface ISubObjectA
    {
        void Verify(string containerName);
    }

    public class SubObjectA : ISubObjectA
    {
        public IServiceA ServiceA { get; set; }

        public void Verify(string containerName)
        {
            if (this.ServiceA == null)
            {
                throw new Exception("ServiceA was null for SubObjectC for container " + containerName);
            }
        }
    }
}
