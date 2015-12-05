/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

namespace CuteAnt.Model
{
	/// <summary>服务接口。</summary>
	/// <remarks>服务代理XAgent可以附加代理实现了IServer接口的服务。</remarks>
	public interface IServer
	{
		/// <summary>开始</summary>
		void Start();

		/// <summary>停止</summary>
		void Stop();
	}
}