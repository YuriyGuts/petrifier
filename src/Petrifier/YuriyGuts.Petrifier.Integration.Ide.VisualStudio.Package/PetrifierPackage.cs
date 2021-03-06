﻿using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Package.CustomEditors;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Package
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>

    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    
    // This attribute is used to register the informations needed to show the this package in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // This attribute is needed to let the shell know that this package provides custom editors.
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a703-06e5-11d1-8ebd-00a0c90f26ea}")]
    [ProvideEditorExtension(typeof(EditorFactory), ".pnml", 50, 
        ProjectGuid = "{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3}", 
        TemplateDir = "Templates", 
        NameResourceID = 105,
        DefaultName = "Petrifier")]

    // This attribute tells that the package provides additional keyboard mappings.
    [ProvideKeyBindingTable(PackageGuids.PackageEditorFactoryGuidString, 102)]
    
    // This attribute declares the GUID for the package.
    [Guid(PackageGuids.PackageGuidString)]
    
    // This attribute notifies that the package provides custom toolbox items.
    [ProvideToolboxItems(1)]

    public sealed class PetrifierPackage : Microsoft.VisualStudio.Shell.Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public PetrifierPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this));
        }

        //
        // Overridden Package Implementation
        //
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this));
            base.Initialize();

            // Create Editor Factory. Note that the base Package class will call Dispose on it.
            RegisterEditorFactory(new EditorFactory(this));
        }

        #endregion
    }
}
