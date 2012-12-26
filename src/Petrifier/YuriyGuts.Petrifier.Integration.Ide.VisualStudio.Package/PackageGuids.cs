// Guids.cs
// MUST match guids.h
using System;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Package
{
    static class PackageGuids
    {
        public const string PackageGuidString = "8e45ef25-61a4-4b3d-a4a3-aeaa105d84a0";
        public const string PackageCmdSetGuidString = "758c3593-9f10-4112-90ee-46eb39c78501";
        public const string PackageEditorFactoryGuidString = "bb7ada40-357d-48c6-bb36-9b353b8a99ba";

        public static readonly Guid PackageCmdSetGuid = new Guid(PackageCmdSetGuidString);
        public static readonly Guid PackageEditorFactoryGuid = new Guid(PackageEditorFactoryGuidString);
    };
}