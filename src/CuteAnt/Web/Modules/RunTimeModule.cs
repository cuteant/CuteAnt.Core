﻿using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CuteAnt.Log;

namespace CuteAnt.Web
{
	/// <summary>页面执行时间模块</summary>
	public class RunTimeModule : IHttpModule
	{
		#region IHttpModule Members

		void IHttpModule.Dispose()
		{
		}

		/// <summary>初始化模块，准备拦截请求。</summary>
		/// <param name="context"></param>
		void IHttpModule.Init(HttpApplication context)
		{
			if (!IsWriteRunTime) { return; }
			context.BeginRequest += new EventHandler(OnBeginRequest);
			context.PostReleaseRequestState += new EventHandler(WriteRunTime);
		}

		private void OnBeginRequest(Object sender, EventArgs e)
		{
			OnInit();
		}

		/// <summary>初始化</summary>
		protected virtual void OnInit()
		{
		}

		#endregion

		#region 属性

		/// <summary>上下文</summary>
		public static HttpContext Context { get { return HttpContext.Current; } }

		/// <summary>请求</summary>
		public static HttpRequest Request { get { return HttpContext.Current.Request; } }

		/// <summary>响应</summary>
		public static HttpResponse Response { get { return HttpContext.Current.Response; } }

		#endregion

		#region 运行时输出

		/// <summary>当前请求是否输出执行时间</summary>
		/// <remarks>如果要所有请求不输出执行时间，则从配置中移除当前模块</remarks>
		public static Boolean IsWriteRunTime
		{
			get
			{
				Object obj = Context.Items["IsWriteRunTime"];
				return (obj is Boolean) ? (Boolean)obj : HmTrace.Debug;
			}
			set { Context.Items["IsWriteRunTime"] = value; }
		}

		/// <summary>当前请求是否已经输出执行时间</summary>
		public static Boolean HasWrite
		{
			get
			{
				Object obj = Context.Items["HasWrite"];
				return (obj is Boolean) ? (Boolean)obj : false;
			}
			set { Context.Items["HasWrite"] = value; }
		}

		private static String _RunTimeFormat = "页面执行时间{0}毫秒！";

		/// <summary>执行时间字符串</summary>
		public static String RunTimeFormat { get { return _RunTimeFormat; } set { _RunTimeFormat = value; } }

		/// <summary>输出运行时间</summary>
		private void WriteRunTime(Object sender, EventArgs e)
		{
			if (!IsWriteRunTime) { return; }

			if (!Request.PhysicalPath.EndsWithIgnoreCase(".aspx")) { return; }

			//判断是否为Ajax 异步请求，以排除“Sys.WebForms.PageRequestManagerParserErrorException: 未能分析从服务器收到的消息 ”异常
			if (Request.Headers["X-MicrosoftAjax"] != null || Request.Headers["x-requested-with"] != null) { return; }
			if (HasWrite) { return; }
			HasWrite = true;

			// 只处理Page页面
			Page page = Context.Handler as Page;
			if (page == null) { return; }
			String str = Render();
			if (str.IsNullOrWhiteSpace()) { return; }

			// 尝试找到页面，并在页面上写上信息
			Literal lt = page.FindControl("RunTime") as Literal;
			if (lt != null)
				lt.Text = str;
			else
				Response.Write(str);
		}

		/// <summary>输出</summary>
		/// <returns></returns>
		protected virtual String Render()
		{
			TimeSpan ts = DateTime.Now - HttpContext.Current.Timestamp;
			return String.Format(RunTimeFormat, ts.TotalMilliseconds);
		}

		#endregion
	}
}