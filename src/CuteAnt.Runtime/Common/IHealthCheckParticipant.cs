using System;

namespace CuteAnt.Runtime
{
  public interface IHealthCheckParticipant
  {
    bool CheckHealth(DateTime lastCheckTime);
  }
}
