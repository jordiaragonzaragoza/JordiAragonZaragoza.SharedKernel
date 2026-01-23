// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1210:Using directives should be ordered alphabetically by namespace", Justification = "Temporal suppresion")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1015:Closing generic brackets should be spaced correctly", Justification = "C#11 Not required for generic on attributes")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Temporal suppresion")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "Temporal suppresion")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Temporal suppresion")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "Temporal suppresion")]

[assembly: SuppressMessage("Info Code Smell", "S1135:Track uses of \"TODO\" tags", Justification = "Temporal suppresion")]
[assembly: SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Temporal suppresion")]
[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Temporal suppresion")]

[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Warning not applicable to .NET Core")]
[assembly: SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = "Ok for empty records")]

[assembly: SuppressMessage("Minor Code Smell", "S3236:Caller information arguments should not be provided explicitly", Justification = "Ok for Ardalis.GuardClauses")]
[assembly: SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Required to find private constructors on reflection")]
[assembly: SuppressMessage("Major Code Smell", "S2139:Exceptions should be either logged or rethrown but not both", Justification = "Required for application pipelines")]