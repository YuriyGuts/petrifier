using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Package.CustomEditors
{
    /// <summary>
    /// Factory for creating the editor object. Extends from the IVsEditoryFactory interface.
    /// </summary>
    [Guid(PackageGuids.PackageEditorFactoryGuidString)]
    public sealed class EditorFactory : IVsEditorFactory, IDisposable
    {
        private readonly PetrifierPackage editorPackage;
        private ServiceProvider vsServiceProvider;

        public EditorFactory(PetrifierPackage package)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} constructor", this));
            editorPackage = package;
        }

        /// <summary>
        /// Since we create a ServiceProvider which implements IDisposable we
        /// also need to implement IDisposable to make sure that the ServiceProvider's
        /// Dispose method gets called.
        /// </summary>
        public void Dispose()
        {
            if (vsServiceProvider != null)
            {
                vsServiceProvider.Dispose();
            }
        }

        #region IVsEditorFactory Members

        /// <summary>
        /// Used for initialization of the editor in the environment
        /// </summary>
        /// <param name="psp">pointer to the service provider. Can be used to obtain instances of other interfaces
        /// </param>
        /// <returns></returns>
        public int SetSite(IServiceProvider psp)
        {
            vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public object GetService(Type serviceType)
        {
            return vsServiceProvider.GetService(serviceType);
        }

        //
        // This method is called by the Environment (inside IVsUIShellOpenDocument::
        // OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a PHYSICAL view.
        // A LOGICAL view identifies the purpose of the view that is
        // desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a 
        // view appropriate for text view manipulation as by navigating to a find
        // result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type 
        // of view implementation that an IVsEditorFactory can create. 
        
        // NOTE: Physical views are identified by a string of your choice with the 
        // one constraint that the default/primary physical view for an editor  
        // *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
        
        // NOTE: It is essential that the implementation of MapLogicalView properly
        // validates that the LogicalView desired is actually supported by the editor.
        // If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
        
        // NOTE: The special Logical Views supported by an Editor Factory must also 
        // be registered in the local registry hive. LOGVIEWID_Primary is implicitly 
        // supported by all editor types and does not need to be registered.
        // For example, an editor that supports a ViewCode/ViewDesigner scenario
        // might register something like the following:
        //         HKLM\Software\Microsoft\VisualStudio\<version>\Editors\
        //             {...guidEditor...}\
        //                 LogicalViews\
        //                     {...LOGVIEWID_TextView...} = s ''
        //                     {...LOGVIEWID_Code...} = s ''
        //                     {...LOGVIEWID_Debugging...} = s ''
        //                     {...LOGVIEWID_Designer...} = s 'Form'
        //
        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;

            // Only a single physical view is supported.
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
            {
                // Primary view uses NULL as pbstrPhysical View.
                return VSConstants.S_OK;
            }

            // Any unrecognized rguidLogicalView leads to Not Implemented exception.
            return VSConstants.E_NOTIMPL;
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Used by the editor factory to create an editor instance. the environment first determines the 
        /// editor factory with the highest priority for opening the file and then calls 
        /// IVsEditorFactory.CreateEditorInstance. If the environment is unable to instantiate the document data 
        /// in that editor, it will find the editor with the next highest priority and attempt to so that same thing. 
        /// NOTE: The priority of our editor is 32 as mentioned in the attributes on the package class.
        /// 
        /// Since our editor supports opening only a single view for an instance of the document data, if we 
        /// are requested to open document data that is already instantiated in another editor, or even our 
        /// editor, we return a value VS_E_INCOMPATIBLEDOCDATA.
        /// </summary>
        /// <param name="grfCreateDoc">Flags determining when to create the editor. Only open and silent flags are valid.</param>
        /// <param name="pszMkDocument">Path to the file to be opened.</param>
        /// <param name="pszPhysicalView">Name of the physical view.</param>
        /// <param name="pvHier">Pointer to the IVsHierarchy interface.</param>
        /// <param name="itemid">Item identifier of this editor instance.</param>
        /// <param name="punkDocDataExisting">This parameter is used to determine if a document buffer (DocData object) has already been created.</param>
        /// <param name="ppunkDocView">Pointer to the IUnknown interface for the DocView object.</param>
        /// <param name="ppunkDocData">Pointer to the IUnknown interface for the DocData object.</param>
        /// <param name="pbstrEditorCaption">Caption mentioned by the editor for the doc window.</param>
        /// <param name="pguidCmdUI">the Command UI Guid. Any UI element that is visible in the editor has to use this GUID. This is specified in the .vsct file.</param>
        /// <param name="pgrfCDW">Flags for CreateDocumentWindow.</param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public int CreateEditorInstance(
            uint grfCreateDoc,
            string pszMkDocument,
            string pszPhysicalView,
            IVsHierarchy pvHier,
            uint itemid,
            IntPtr punkDocDataExisting,
            out IntPtr ppunkDocView,
            out IntPtr ppunkDocData,
            out string pbstrEditorCaption,
            out Guid pguidCmdUI,
            out int pgrfCDW)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} CreateEditorInstace()", this));

            // Initialize out-parameters to null.
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = PackageGuids.PackageEditorFactoryGuid;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs.
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Create the Document (editor).
            PetriNetEditorPane NewEditor = new PetriNetEditorPane(editorPackage);
            ppunkDocView = Marshal.GetIUnknownForObject(NewEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(NewEditor);
            pbstrEditorCaption = string.Empty;

            return VSConstants.S_OK;
        }

        #endregion
    }
}
