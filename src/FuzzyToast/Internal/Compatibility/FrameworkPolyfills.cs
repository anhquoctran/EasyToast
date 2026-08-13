#if !NET5_0_OR_GREATER
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class IsExternalInit
	{
	}
}

namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class NotNullWhenAttribute : Attribute
	{
		public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
		public bool ReturnValue { get; }
	}
}
#endif

#if !NET7_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	internal sealed class CompilerFeatureRequiredAttribute : Attribute
	{
		public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

		public string FeatureName { get; }
		public bool IsOptional { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	internal sealed class RequiredMemberAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
	internal sealed class SetsRequiredMembersAttribute : Attribute
	{
	}
}
#endif
