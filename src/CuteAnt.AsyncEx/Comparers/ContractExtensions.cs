//
//  Include this file in your project if your project uses
//  ContractArgumentValidator or ContractAbbreviator methods
//

namespace System.Diagnostics.Contracts
{
	///// <summary>
	///// Enables factoring legacy if-then-throw into separate methods for reuse and full control over
	///// thrown exception and arguments
	///// </summary>
	//[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	//[Conditional("CONTRACTS_FULL")]
	//internal sealed class ContractArgumentValidatorAttribute : global::System.Attribute
	//{
	//}

	///// <summary>
	///// Enables writing abbreviations for contracts that get copied to other methods
	///// </summary>
	//[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	//[Conditional("CONTRACTS_FULL")]
	//internal sealed class ContractAbbreviatorAttribute : global::System.Attribute
	//{
	//}

#if !NET_4_0_GREATER
	/// <summary>Allows setting contract and tool options at assembly, type, or method granularity.</summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	[Conditional("CONTRACTS_FULL")]
	internal sealed class ContractOptionAttribute : global::System.Attribute
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "category", Justification = "Build-time only attribute")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "setting", Justification = "Build-time only attribute")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "enabled", Justification = "Build-time only attribute")]
		public ContractOptionAttribute(String category, String setting, Boolean enabled) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "category", Justification = "Build-time only attribute")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "setting", Justification = "Build-time only attribute")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "Build-time only attribute")]
		public ContractOptionAttribute(String category, String setting, String value) { }
	}
#endif
}