using System;
using System.Collections.Generic;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal sealed class CompareSequences
	{
		#region -- Fields --

		private readonly SchemaProvider _SchemaProvider;
		private readonly IList<CompareResult> _Results;
		private readonly GeneratorBase _Generator;

		#endregion

		#region -- 构造 --

		internal CompareSequences(SchemaProvider schemaProvider, GeneratorBase generator, IList<CompareResult> _results)
		{
			_SchemaProvider = schemaProvider;
			_Generator = generator;
			_Results = _results;
		}

		#endregion
	}
}
