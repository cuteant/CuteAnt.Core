#if !NET40
using System;
using System.Threading.Tasks;

namespace CuteAnt.Runtime
{
  internal sealed class AsynchAgentTask : Task
  {
    public readonly string Name;

    public AsynchAgentTask(Action action, string name) : base(action)
    {
      Name = name;
    }
  }
}
#endif
