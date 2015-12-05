﻿using System;
using System.IO.Compression;
using System.Linq;
using System.Web;
using CuteAnt.Configuration;
using CuteAnt.Reflection;

namespace CuteAnt.Web
{
	/// <summary>页面压缩模块</summary>
	public class CompressionModule : IHttpModule
	{
		#region IHttpModule Members

		void IHttpModule.Dispose()
		{
		}

		/// <summary>初始化模块，准备拦截请求。</summary>
		/// <param name="context"></param>
		void IHttpModule.Init(HttpApplication context)
		{
			context.PostReleaseRequestState += CompressContent;
		}

		#endregion

		#region Compression

		private const String GZIP = "gzip";
		private const String DEFLATE = "deflate";

		private void CompressContent(Object sender, EventArgs e)
		{
			var app = sender as HttpApplication;
			if (!(app.Context.CurrentHandler is System.Web.UI.Page) || app.Request["HTTP_X_MICROSOFTAJAX"] != null) { return; }

			// 如果已经写入头部，这里就不能压缩了
			Object rs = null;
			if (app.Response.TryGetValue("HeadersWritten", out rs) && (Boolean)rs) { return; }
			// .net 2.0没有HeadersWritten

			//压缩
			String url = app.Request.Url.OriginalString.ToLower();
			if (CanCompress(url))
			{
				//是否支持压缩协议
				if (IsEncodingAccepted(GZIP))
				{
					app.Response.Filter = new GZipStream(app.Response.Filter, CompressionMode.Compress);
					SetEncoding(GZIP);
				}
				else if (IsEncodingAccepted(DEFLATE))
				{
					app.Response.Filter = new DeflateStream(app.Response.Filter, CompressionMode.Compress);
					SetEncoding(DEFLATE);
				}
			}
		}

		private String[] exts;

		/// <summary>是否可压缩文件</summary>
		/// <param name="url"></param>
		/// <returns></returns>
		protected virtual Boolean CanCompress(String url)
		{
			if (exts == null)
			{
				String files = Config.GetMutilConfig<String>(".aspx,.axd,.js,.css", "CuteAnt.Web.CompressFiles", "CuteAnt.Data.CommonEntity.CompressFiles");
				exts = files.ToLower().Split(",", ";", " ");
			}
			return exts.Any(t => url.Contains(t));
		}

		/// <summary>检查请求头，确认客户端是否支持压缩编码</summary>
		private static Boolean IsEncodingAccepted(String encoding)
		{
			return HttpContext.Current.Request.Headers["Accept-encoding"] != null && HttpContext.Current.Request.Headers["Accept-encoding"].Contains(encoding);
		}

		/// <summary>添加压缩编码到响应头</summary>
		/// <param name="encoding"></param>
		private static void SetEncoding(String encoding)
		{
			HttpContext.Current.Response.AppendHeader("Content-encoding", encoding);
		}

		#endregion
	}
}