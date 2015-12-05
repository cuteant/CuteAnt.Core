using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuteAnt.AsyncEx.Dataflow
{
	/// <summary>ISelfTransformation</summary>
	/// <typeparam name="TOutput"></typeparam>
	public interface ISelfTransformation<TOutput>
	{
		/// <summary>TransformAsync</summary>
		/// <param name="storeOutputItem"></param>
		/// <returns></returns>
		Task TransformAsync(Action<TOutput> storeOutputItem);
	}

	//public interface ISyncSelfTransformation<TOutput> : ISelfTransformation
	//{
	//	void Transform(Action<TOutput> storeOutputItem);
	//}

	//public interface IAsyncSelfTransformation<TOutput> : ISelfTransformation
	//{
	//	Task TransformAsync(Action<TOutput> storeOutputItem);
	//}
}
