using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CuteAnt.IO;
using CuteAnt.OrmLite.Sprite;

namespace CuteAnt.OrmLite.Testing
{
  public class Program
  {
    public static void Main(string[] args)
    {
      try
      {
        if (TestingCombGuid.Meta.Count <= 0)
        {
          for (int i = 0; i < 1000; i++)
          {
            var user = new TestingCombGuid();
            user.ID = CombGuid.NewComb();
            user.Name = "Name" + i;
            user.CreateOn = DateTime.Now;
            user.CreateBy = "Administrator";
            user.CreateUserID = 1;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = "Administrator";
            user.ModifiedUserID = 1;
            user.Insert();
          }
        }
        if (TestingLong.Meta.Count <= 0)
        {
          for (int i = 0; i < 1000; i++)
          {
            var user = new TestingLong();
            //user.ID = CombGuid.NewComb();
            user.Name = "Name" + i;
            user.CreateOn = DateTime.Now;
            user.CreateBy = "Administrator";
            user.CreateUserID = 1;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = "Administrator";
            user.ModifiedUserID = 1;
            user.Insert();
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      Console.WriteLine(TestingCombGuid.Meta.Count);
      var list = TestingCombGuid.FindAll(null, null, null, 0, 10);
      foreach (var item in list)
      {
        Console.WriteLine(item.ID);
        Console.WriteLine(item.Name);
      }

      var dataModel = DataModel.FindByID(1);

      using (var fs = new FileStream(PathHelper.ApplicationBasePathCombine("sprite.xml"), FileMode.Create))
      {
        dataModel.ExportXml(fs);
      }

      Console.WriteLine("按任意键退出！");
      Console.ReadKey();
    }
  }
}
