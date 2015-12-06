/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Web;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Web;

namespace CuteAnt.OrmLite.Web
{
	/// <summary>页面查询执行时间模块</summary>
	public class DbRunTimeModule : RunTimeModule
	{
		//static DbRunTimeModule()
		//{
		//    RunTimeFormat = "查询{0}次，执行{1}次，耗时{2}毫秒！";
		//}

		private static String _RunTimeFormat = "查询{0}次，执行{1}次，耗时{2:n0}毫秒！";

		/// <summary>执行时间字符串</summary>
		public static String DbRunTimeFormat
		{
			get { return _RunTimeFormat; }
			set { _RunTimeFormat = value; }
		}

		/// <summary>初始化模块，准备拦截请求。</summary>
		protected override void OnInit()
		{
			Context.Items["DAL.QueryTimes"] = DAL.QueryTimes;
			Context.Items["DAL.ExecuteTimes"] = DAL.ExecuteTimes;
		}

		/// <summary>输出</summary>
		/// <returns></returns>
		protected override String Render()
		{
			TimeSpan ts = DateTime.Now - HttpContext.Current.Timestamp;
			Int32 StartQueryTimes = (Int32)Context.Items["DAL.QueryTimes"];
			Int32 StartExecuteTimes = (Int32)Context.Items["DAL.ExecuteTimes"];
			return String.Format(DbRunTimeFormat, DAL.QueryTimes - StartQueryTimes, DAL.ExecuteTimes - StartExecuteTimes, ts.TotalMilliseconds);
		}
	}
}