using System;
using System.Collections.Generic;
using System.Text;
using CuteAnt.OrmLite;

namespace CuteAnt.OrmLite.Testing
{
  public class XiaoMIShard : ShardingProvider<xiaomi, Int32>
  {
    public XiaoMIShard()
    {
      DbProviderName = "MsSql";
    }

    public XiaoMIShard(String dbProviderName)
    {
      DbProviderName = dbProviderName;
    }

    public override void StartupBySerializedShardingKey(String shardingKey)
    {
    }

    public override void Startup(Int32 shardingKey)
    {
    }
  }

  public class XiaoMIShardFactory : ShardingProviderFactory<XiaoMIShardFactory, XiaoMIShard, xiaomi, Int32> { }
}
