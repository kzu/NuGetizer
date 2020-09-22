﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
    public class TargetsTests
	{
		ITestOutputHelper output;

		public TargetsTests(ITestOutputHelper output) => this.output = output;

		[Fact]
		public void IncludeFrameworkReferencesInPackage_is_not_true_for_build_primary_output()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "PrimaryOutputKind", "build" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_is_not_true_for_tool_primary_output()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "PrimaryOutputKind", "tool" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_is_not_true_for_tools_primary_output()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "PrimaryOutputKind", "tools" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_is_true_for_primary_output_lib()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "PrimaryOutputKind", "lib" }
			}, null);

			Assert.Equal("true", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_is_true_for_default_primary_output_kind()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
			}, null);

			Assert.Equal("true", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_is_not_true_for_compat_istool()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "IsTool", "true" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void PackOnBuild_defaults_to_true_for_compat_GeneratePackageOnBuild_true()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "GeneratePackageOnBuild", "true" }
			}, null);

			Assert.Equal("true", project.GetPropertyValue("PackOnBuild"));
		}

		[Fact]
		public void package_contents_never_includes_nugetizer_package_reference()
		{
			var xml = ProjectRootElement.Create(Path.Combine(Directory.GetCurrentDirectory(), MethodBase.GetCurrentMethod().Name));
			xml.AddImport(@"Scenarios\Scenario.props");
			xml.AddImport(@"Scenarios\Scenario.targets");
			xml.AddItemGroup()
				.AddItem("PackageReference", "NuGet.Build.Packaging", new[] { new KeyValuePair<string, string>("Version", "*") });

			xml.Save();

			var project = new ProjectInstance(xml);

			var result = Builder.Build(project, "GetPackageContents");

			Assert.Equal(BuildResultCode.Success, result.OverallResult);
			Assert.Equal(TargetResultCode.Success, result["GetPackageContents"].ResultCode);

			var items = result["GetPackageContents"].Items;

			Assert.DoesNotContain(items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "NuGet.Build.Packaging",
			}));
		}
	}
}
