/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

namespace CuteAnt.Threading
{
	/// <summary>任务状态</summary>
	public enum TaskState
	{
		/// <summary>未处理</summary>
		Unstarted = 0,

		/// <summary>正在处理</summary>
		Running = 1,

		/// <summary>已完成</summary>
		Finished = 2
	}
}