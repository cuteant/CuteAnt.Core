using System.Collections;

namespace CuteAnt.Comparers
{
	/// <summary>A non-generic comparer that also provides equality comparison (and hash codes).</summary>
	public interface IFullComparer : IComparer, IEqualityComparer
	{
	}
}