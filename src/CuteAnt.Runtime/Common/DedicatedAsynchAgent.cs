#if !NET40

namespace CuteAnt.Runtime
{
  public abstract class DedicatedAsynchAgent : AsynchAgent
  {
    protected DedicatedAsynchAgent() : base() { }

    protected DedicatedAsynchAgent(string nameSuffix) : base(nameSuffix) { }

    public override void OnStart()
    {
      executor.QueueWorkItem(_ => Run());
    }

    protected abstract void Run();
  }
}
#endif
