using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SourceBrowser.HtmlGenerator.Utilities
{
	using Microsoft.CodeAnalysis;

	public static class ProjectUtils
	{
		public static string GetAssemblyName(this Project project) {
			return project.Name.EndsWith("(netstandard2.0)", StringComparison.OrdinalIgnoreCase)
				? project.Name
				: project.AssemblyName;
		}
	}
}
