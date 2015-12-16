﻿#if DESKTOPCLR
/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
*/
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using CuteAnt.Log;

namespace CuteAnt.Web
{
	/// <summary>扩展的Web客户端</summary>
	public class WebClientX : WebClient
	{
		static WebClientX()
		{
			// 设置默认最大连接为20，关闭默认代理，提高响应速度
			ServicePointManager.DefaultConnectionLimit = 20;
			WebRequest.DefaultWebProxy = null;
		}

		#region 为了Cookie而重写
		private CookieContainer _Cookie;
		/// <summary>Cookie容器</summary>
		public CookieContainer Cookie { get { return _Cookie ?? (_Cookie = new CookieContainer()); } set { _Cookie = value; } }

		#endregion

		#region 属性
		private String _Accept;
		/// <summary>可接受类型</summary>
		public String Accept { get { return _Accept; } set { _Accept = value; } }

		private String _AcceptLanguage;
		/// <summary>可接受语言</summary>
		public String AcceptLanguage { get { return _AcceptLanguage; } set { _AcceptLanguage = value; } }

		private String _Referer;
		/// <summary>引用页面</summary>
		public String Referer { get { return _Referer; } set { _Referer = value; } }

		private Int32 _Timeout;
		/// <summary>超时，毫秒</summary>
		public Int32 Timeout { get { return _Timeout; } set { _Timeout = value; } }

		private DecompressionMethods _AutomaticDecompression;
		/// <summary>自动解压缩模式。</summary>
		public DecompressionMethods AutomaticDecompression { get { return _AutomaticDecompression; } set { _AutomaticDecompression = value; } }

		private String _UserAgent;
		/// <summary>User-Agent 标头，指定有关客户端代理的信息</summary>
		public String UserAgent { get { return _UserAgent; } set { _UserAgent = value; } }
		#endregion

		#region 构造
		/// <summary>实例化</summary>
		public WebClientX() { }

		/// <summary>初始化常用的东西</summary>
		/// <param name="ie">是否模拟ie</param>
		/// <param name="iscompress">是否压缩</param>
		public WebClientX(Boolean ie, Boolean iscompress)
		{
			if (ie)
			{
				Accept = "text/html, */*";
				AcceptLanguage = "zh-CN";
				//Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
				var name = Assembly.GetEntryAssembly().GetName().Name;
				UserAgent = "Mozilla/5.0 (compatible; MSIE 11.0; Windows NT 6.1; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E; {0})".FormatWith(name);
			}
			if (iscompress) AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
		}
		#endregion

		#region 重载设置属性
		/// <summary>重写获取请求</summary>
		/// <param name="address"></param>
		/// <returns></returns>
		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address);

			var hr = request as HttpWebRequest;
			if (hr != null)
			{
				hr.CookieContainer = Cookie;
				hr.AutomaticDecompression = AutomaticDecompression;

				if (!String.IsNullOrEmpty(Accept)) hr.Accept = Accept;
				if (!String.IsNullOrEmpty(AcceptLanguage)) hr.Headers[HttpRequestHeader.AcceptLanguage] = AcceptLanguage;
				if (!String.IsNullOrEmpty(UserAgent)) hr.UserAgent = UserAgent;
				if (!String.IsNullOrEmpty(Accept)) hr.Accept = Accept;
			}

			if (Timeout > 0) request.Timeout = Timeout;

			return request;
		}

		/// <summary>重写获取响应</summary>
		/// <param name="request"></param>
		/// <returns></returns>
		protected override WebResponse GetWebResponse(WebRequest request)
		{
			var response = base.GetWebResponse(request);
			var http = response as HttpWebResponse;
			if (http != null)
			{
				Cookie.Add(http.Cookies);
				if (!String.IsNullOrEmpty(http.CharacterSet)) Encoding = System.Text.Encoding.GetEncoding(http.CharacterSet);
			}

			return response;
		}
		#endregion

		#region 方法
		/// <summary>获取指定地址的Html，自动处理文本编码</summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public String GetHtml(String url)
		{
			var buf = DownloadData(url);
			Referer = url;
			if (buf == null || buf.Length == 0) return null;

			// 处理编码
			var enc = Encoding;
			//if (ResponseHeaders[HttpResponseHeader.ContentType].Contains("utf-8")) enc = System.Text.Encoding.UTF8;

			return buf.ToStr(enc);
		}

		/// <summary>获取指定地址的Html，分析所有超链接</summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public Link[] GetLinks(String url)
		{
			var html = GetHtml(url);
			if (html.IsNullOrWhiteSpace()) return new Link[0];

			return Link.Parse(html, url);
		}

		/// <summary>分析指定页面指定名称的链接，并下载到目标目录，返回目标文件</summary>
		/// <remarks>
		/// 根据版本或时间降序排序选择
		/// </remarks>
		/// <param name="url">指定页面</param>
		/// <param name="name">页面上指定名称的链接</param>
		/// <param name="destdir">要下载到的目标目录</param>
		/// <returns>返回已下载的文件，无效时返回空</returns>
		public String DownloadLink(String url, String name, String destdir)
		{
			var ls = GetLinks(url);
			if (ls.Length == 0) return null;

			// 过滤名称后降序排序
			ls = ls.Where(e => e.Name.StartsWithIgnoreCase(name) || e.Name.Contains(name))
					.Where(e => !e.Url.IsNullOrWhiteSpace())
					.OrderByDescending(e => e.Version)
					.OrderByDescending(e => e.Time)
					.ToArray();
			if (ls.Length == 0) return null;

			var link = ls[0];
			HmTrace.WriteDebug("分析得到文件 {0}，准备下载 {1}", link.Name, link.Url);
			// 开始下载文件，注意要提前建立目录，否则会报错
			var file = destdir.CombinePath(link.Name).EnsureDirectory();
			DownloadFile(link.Url, file);
			return file;
		}
		#endregion
	}
}
#endif