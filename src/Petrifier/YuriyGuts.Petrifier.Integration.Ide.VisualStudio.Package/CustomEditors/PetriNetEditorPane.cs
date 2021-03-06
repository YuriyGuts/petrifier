﻿using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using YuriyGuts.Petrifier.Core.PetriNets;
using YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Controls;
using YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Toolbox;
using YuriyGuts.Petrifier.Persistence;
using YuriyGuts.Petrifier.Persistence.Pnml;
using YuriyGuts.Petrifier.Visualization.PetriNets;
using YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Code;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Package.CustomEditors
{
    /// <summary>
    /// This control host the editor and is responsible for handling the commands targeted
    /// to the editor as well as saving and loading the document.
    /// This control also implement the search and replace functionalities.
    /// </summary>

    [ComVisible(true)]
    public sealed class PetriNetEditorPane : WindowPane,
        IEditor,                      // the automation interface for Editor
        IExtensibleObject,            // so we can get the automation object
        IPersistFileFormat,           // to enable the programmatic loading or saving of an object in a format specified by the user
        IVsPersistDocData,            // to Enable persistence functionality for document data
        IVsFileChangeEvents,          // to notify the client when file changes on disk
        IVsDocDataFileChangeControl,  // to Determine whether changes to files made outside of the editor should be ignored
        IVsFileBackup,                // to support backup of files (Visual Studio File Recovery backs up all objects in the Running Document Table)
        IVsStatusbarUser,             // support updating the status bar
        IVsToolboxUser                // sends notification about Toolbox items to the owner of these items
    {
        private const uint pnmlFormat = 0;
        private const string pnmlExtension = ".pnml";

        private class EditorProperties
        {
            private PetriNetEditorPane editor;
            public EditorProperties(PetriNetEditorPane Editor)
            {
                editor = Editor;
            }

            public string FileName
            {
                get { return editor.FileName; }
            }

            public bool DataChanged
            {
                get { return editor.DataChanged; }
            }
        }

        #region Fields

        private PetrifierPackage package;
        private PetriNetDesigner editorControl;

        private IToolboxService toolboxService;

        private string fileName = string.Empty;
        private bool isDirty;

        // Flag true when we are loading the file. It is used to avoid to change the isDirty flag
        // when the changes are related to the load operation.
        private bool loading;

        // This flag is true when we are asking the QueryEditQuerySave service if we can edit the
        // file. It is used to avoid to have more than one request queued.
        private bool gettingCheckoutStatus;

        private Microsoft.VisualStudio.Shell.SelectionContainer selContainer;
        private ITrackSelection trackSel;
        private IVsFileChangeEx vsFileChangeEx;
        private IVsWindowFrame frame;

        private Timer FileChangeTrigger = new Timer();
        private Timer FNFStatusbarTrigger = new Timer();

        private bool fileChangedTimerSet;
        private int ignoreFileChangeLevel;
        private bool backupObsolete = true;
        private uint vsFileChangeCookie;

        private IExtensibleObjectSite extensibleObjectSite;

        #endregion

        #region "Window.Pane Overrides"
        
        /// <summary>
        /// Constructor that calls the Microsoft.VisualStudio.Shell.WindowPane constructor then
        /// our initialization functions.
        /// </summary>
        /// <param name="package">Our Package instance.</param>
        public PetriNetEditorPane(PetrifierPackage package)
            : base(null)
        {
            PrivateInit(package);
        }

        protected override void Initialize()
        {
            base.Initialize();
            InitializeToolboxItems();
        }

        public void InitializeToolboxItems()
        {
            OnRefreshToolbox(this, EventArgs.Empty);
        }

        private void OnRefreshToolbox(object sender, EventArgs e)
        {
            toolboxService = (IToolboxService)GetService(typeof(IToolboxService));
            IVsToolbox vsToolbox = GetService(typeof(IVsToolbox)) as IVsToolbox;

            if (toolboxService == null || vsToolbox == null)
            {
                return;
            }

            PetriNetEditorToolboxProvider toolboxProvider = new PetriNetEditorToolboxProvider();
            var itemList = toolboxProvider.GetToolboxItemList();

            if (itemList == null || itemList.Count == 0)
            {
                return;
            }

            string toolboxCategoryName = PetriNetEditorToolboxProvider.ToolboxCategoryName;

            //Remove target tab and all controls under it.
            foreach (ToolboxItem oldItem in toolboxService.GetToolboxItems(toolboxCategoryName))
            {
                toolboxService.RemoveToolboxItem(oldItem);
            }
            vsToolbox.RemoveTab(PetriNetEditorToolboxProvider.ToolboxCategoryName);

            foreach (ToolboxItem item in itemList)
            {
                toolboxService.AddToolboxItem(item, toolboxCategoryName);
            }

            toolboxService.SelectedCategory = toolboxCategoryName;
            toolboxService.Refresh();
        }

        /// <summary>
        /// This is a required override from the Microsoft.VisualStudio.Shell.WindowPane class.
        /// It returns the extended rich text box that we host.
        /// </summary>
        public override IWin32Window Window
        {
            get
            {
                return this.editorControl;
            }
        }
        #endregion

        /// <summary>
        /// Initialization routine for the Editor. Loads the list of properties for the pnml document 
        /// which will show up in the properties window 
        /// </summary>
        /// <param name="package"></param>
        private void PrivateInit(PetrifierPackage package)
        {
            this.package = package;
            loading = false;
            gettingCheckoutStatus = false;
            trackSel = null;

            Control.CheckForIllegalCrossThreadCalls = false;
            // Create an ArrayList to store the objects that can be selected
            ArrayList listObjects = new ArrayList();

            // Create the object that will show the document's properties
            // on the properties window.
            EditorProperties prop = new EditorProperties(this);
            listObjects.Add(prop);

            // Create the SelectionContainer object.
            selContainer = new Microsoft.VisualStudio.Shell.SelectionContainer(true, false);
            selContainer.SelectableObjects = listObjects;
            selContainer.SelectedObjects = listObjects;

            // Create and initialize the editor

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PetriNetEditorPane));
            editorControl = new PetriNetDesigner();
            editorControl.SelectionChanged += (sender, args) => { isDirty = true; RefreshObjectSelection(); };

            resources.ApplyResources(this.editorControl, "editorControl", CultureInfo.CurrentUICulture);

            // Call the helper function that will do all of the command setup work
            setupCommands();
        }

        /// <summary>
        /// returns the name of the file currently loaded
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// returns whether the contents of file have changed since the last save
        /// </summary>
        public bool DataChanged
        {
            get { return isDirty; }
        }

        /// <summary>
        /// returns an instance of the ITrackSelection service object
        /// </summary>
        private ITrackSelection TrackSelection
        {
            get
            {
                if (trackSel == null)
                {
                    trackSel = (ITrackSelection)GetService(typeof(ITrackSelection));
                }
                return trackSel;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose the timers
                    if (null != FileChangeTrigger)
                    {
                        FileChangeTrigger.Dispose();
                        FileChangeTrigger = null;
                    }
                    if (null != FNFStatusbarTrigger)
                    {
                        FNFStatusbarTrigger.Dispose();
                        FNFStatusbarTrigger = null;
                    }

                    SetFileChangeNotification(null, false);

                    if (editorControl != null)
                    {
                        editorControl.Dispose();
                        editorControl = null;
                    }
                    if (FileChangeTrigger != null)
                    {
                        FileChangeTrigger.Dispose();
                        FileChangeTrigger = null;
                    }
                    if (extensibleObjectSite != null)
                    {
                        extensibleObjectSite.NotifyDelete(this);
                        extensibleObjectSite = null;
                    }
                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            // Lock the document
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            int hr = runningDocTable.FindAndLockDocument(
                (uint)_VSRDTFLAGS.RDT_ReadLock,
                fileName,
                out hierarchy,
                out itemID,
                out docData,
                out docCookie
            );
            ErrorHandler.ThrowOnFailure(hr);

            // Send the notification
            hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

            // Unlock the document.
            // Note that we have to unlock the document even if the previous call failed.
            ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie));

            // Check ff the call to NotifyDocChanged failed.
            ErrorHandler.ThrowOnFailure(hr);
        }

        #region Command Handling Functions

        /// <summary>
        /// This helper function, which is called from the EditorPane's PrivateInit
        /// function, does all the work involving adding commands.
        /// </summary>
        private void setupCommands()
        {
            // Now get the IMenuCommandService; this object is the one
            // responsible for handling the collection of commands implemented by the package.

            IMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (null != mcs)
            {
                // Now create one object derived from MenuCommnad for each command defined in
                // the CTC file and add it to the command service.

                // For each command we have to define its id that is a unique Guid/integer pair, then
                // create the OleMenuCommand object for this command. The EventHandler object is the
                // function that will be called when the user will select the command. Then we add the 
                // OleMenuCommand to the menu service.  The addCommand helper function does all this for us.

                //!!!
                //addCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.SelectAll,
                //                new EventHandler(onSelectAll), null);
            }
        }

        /// <summary>
        /// Helper function used to add commands using IMenuCommandService
        /// </summary>
        /// <param name="mcs"> The IMenuCommandService interface.</param>
        /// <param name="menuGroup"> This guid represents the menu group of the command.</param>
        /// <param name="cmdID"> The command ID of the command.</param>
        /// <param name="commandEvent"> An EventHandler which will be called whenever the command is invoked.</param>
        /// <param name="queryEvent"> An EventHandler which will be called whenever we want to query the status of
        /// the command.  If null is passed in here then no EventHandler will be added.</param>
        private static void addCommand(IMenuCommandService mcs, Guid menuGroup, int cmdID,
                                       EventHandler commandEvent, EventHandler queryEvent)
        {
            // Create the OleMenuCommand from the menu group, command ID, and command event
            CommandID menuCommandID = new CommandID(menuGroup, cmdID);
            OleMenuCommand command = new OleMenuCommand(commandEvent, menuCommandID);

            // Add an event handler to BeforeQueryStatus if one was passed in
            if (null != queryEvent)
            {
                command.BeforeQueryStatus += queryEvent;
            }

            // Add the command using our IMenuCommandService instance
            mcs.AddCommand(command);
        }

        /// <summary>
        /// Handler for when we want to query the status of the clipboard ring.
        /// </summary>
        /// <param name="sender">  This can be cast to an OleMenuCommand.</param>
        /// <param name="e">  Not used.</param>
        private void onQueryPasteNextTBXCBItem(object sender, EventArgs e)
        {
            // Get the Toolbox Service from the package
            IVsToolboxClipboardCycler clipboardCycler = GetService(typeof(SVsToolbox)) as IVsToolboxClipboardCycler;

            int itemsAvailable;
            ErrorHandler.ThrowOnFailure(clipboardCycler.AreDataObjectsAvailable((IVsToolboxUser)this, out itemsAvailable));

            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = ((itemsAvailable > 0) ? true : false);
        }

        /// <summary>
        /// Handler for our Paste command.
        /// </summary>
        /// <param name="sender">  Not used.</param>
        /// <param name="e">  Not used.</param>
        private void onPasteNextTBXCBItem(object sender, EventArgs e)
        {
            // Get the Toolbox Service from the package
            IVsToolboxClipboardCycler clipboardCycler = GetService(typeof(SVsToolbox)) as IVsToolboxClipboardCycler;
            Microsoft.VisualStudio.OLE.Interop.IDataObject pDO;
            ErrorHandler.ThrowOnFailure(clipboardCycler.GetAndSelectNextDataObject((IVsToolboxUser)this, out pDO));
        }

        /// <summary>
        /// This is an extra command handler that we will use to intercept right
        /// mouse click events so that we can call our function to display the
        /// context menu.
        /// </summary>
        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);

                // Convert the point to screen coordinates and pass it into
                // our DisplayContextMenuAt function
                //!!Point screenCoordinates = this.editorControl.RichTextBoxControl.PointToScreen(mouseDownLocation);
                //DisplayContextMenuAt(screenCoordinates);
            }
        }

        /// <summary>
        /// Function that we use to display our context menu.  This function
        /// makes use of the IMenuCommandService's ShowContextMenu function.
        /// </summary>
        /// <param name="point"> The point that we want to display the context menu at.
        /// Note that this must be in screen coordinates.</param>
        private void DisplayContextMenuAt(Point point)
        {
            //!!// Pass in the GUID:ID pair for the context menu.
            //CommandID contextMenuID = new CommandID(PackageGuids.PackageCmdSetGuid, PkgCmdIDList.IDMX_RTF);

            //// Get the OleMenuCommandService from the package
            //IMenuCommandService menuService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            //if (null != menuService)
            //{
            //    // Note: point must be in screen coordinates
            //    menuService.ShowContextMenu(contextMenuID, point.X, point.Y);
            //}
        }

        #endregion

        #region IExtensibleObject Implementation

        /// <summary>
        /// This function is used for Macro playback.  Whenever a macro gets played this funtion will be
        /// called and then the IEditor functions will be called on the object that ppDisp is set to.
        /// Since EditorPane implements IEditor we will just set it to "this".
        /// </summary>
        /// <param name="Name"> Passing in either null, empty string or "Document" will work.  Anything
        /// else will result in ppDisp being set to null.</param>
        /// <param name="pParent"> An object of type IExtensibleObjectSite.  We will keep a reference to this
        /// so that in the Dispose method we can call the NotifyDelete function.</param>
        /// <param name="ppDisp"> The object that this is set to will act as the automation object for macro
        /// playback.  In our case since IEditor is the automation interface and EditorPane
        /// implements it we will just be setting this parameter to "this".</param>
        void IExtensibleObject.GetAutomationObject(string Name, IExtensibleObjectSite pParent, out Object ppDisp)
        {
            // null or empty string just means the default object, but if a specific string
            // is specified, then make sure it's the correct one, but don't enforce case
            if (!string.IsNullOrEmpty(Name) && !Name.Equals("Document", StringComparison.CurrentCultureIgnoreCase))
            {
                ppDisp = null;
                return;
            }

            // Set the out value to this
            ppDisp = (IEditor)this;

            // Store the IExtensibleObjectSite object, it will be used in the Dispose method
            extensibleObjectSite = pParent;
        }

        #endregion

        int Microsoft.VisualStudio.OLE.Interop.IPersist.GetClassID(out Guid pClassID)
        {
            ErrorHandler.ThrowOnFailure(((Microsoft.VisualStudio.OLE.Interop.IPersist)this).GetClassID(out pClassID));
            return VSConstants.S_OK;
        }

        #region IPersistFileFormat Members

        /// <summary>
        /// Notifies the object that it has concluded the Save transaction
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name</param>
        /// <returns>S_OK if the funtion succeeds</returns>
        int IPersistFileFormat.SaveCompleted(string pszFilename)
        {
            // TODO:  Add Editor.SaveCompleted implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the path to the object's current working file 
        /// </summary>
        /// <param name="ppszFilename">Pointer to the file name</param>
        /// <param name="pnFormatIndex">Value that indicates the current format of the file as a zero based index
        /// into the list of formats. Since we support only a single format, we need to return zero. 
        /// Subsequently, we will return a single element in the format list through a call to GetFormatList.</param>
        /// <returns></returns>
        int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            // We only support 1 format so return its index
            pnFormatIndex = pnmlFormat;
            ppszFilename = fileName;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Initialization for the object 
        /// </summary>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the current format 
        /// of the file</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.InitNew(uint nFormatIndex)
        {
            if (nFormatIndex != pnmlFormat)
            {
                return VSConstants.E_INVALIDARG;
            }
            // until someone change the file, we can consider it not dirty as
            // the user would be annoyed if we prompt him to save an empty file
            isDirty = false;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the class identifier of the editor type
        /// </summary>
        /// <param name="pClassID">pointer to the class identifier</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetClassID(out Guid pClassID)
        {
            ErrorHandler.ThrowOnFailure(((Microsoft.VisualStudio.OLE.Interop.IPersist)this).GetClassID(out pClassID));
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provides the caller with the information necessary to open the standard common "Save As" dialog box. 
        /// This returns an enumeration of supported formats, from which the caller selects the appropriate format. 
        /// Each string for the format is terminated with a newline (\n) character. 
        /// The last string in the buffer must be terminated with the newline character as well. 
        /// The first string in each pair is a display string that describes the filter, such as "Text Only 
        /// (*.txt)". The second string specifies the filter pattern, such as "*.txt". To specify multiple filter 
        /// patterns for a single display string, use a semicolon to separate the patterns: "*.htm;*.html;*.asp". 
        /// A pattern string can be a combination of valid file name characters and the asterisk (*) wildcard character. 
        /// Do not include spaces in the pattern string. The following string is an example of a file pattern string: 
        /// "HTML File (*.htm; *.html; *.asp)\n*.htm;*.html;*.asp\nText File (*.txt)\n*.txt\n."
        /// </summary>
        /// <param name="ppszFormatList">Pointer to a string that contains pairs of format filter strings</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetFormatList(out string ppszFormatList)
        {
            char Endline = (char)'\n';
            string FormatList = string.Format(CultureInfo.InvariantCulture, "My Editor (*{0}){1}*{0}{1}{1}", pnmlExtension, Endline);
            ppszFormatList = FormatList;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the file content into the textbox
        /// </summary>
        /// <param name="pszFilename">Pointer to the full path name of the file to load</param>
        /// <param name="grfMode">file format mode</param>
        /// <param name="fReadOnly">determines if teh file should be opened as read only</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            if (pszFilename == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            loading = true;
            int hr = VSConstants.S_OK;
            try
            {
                // Show the wait cursor while loading the file
                IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                if (VsUiShell != null)
                {
                    // Note: we don't want to throw or exit if this call fails, so
                    // don't check the return code.
                    hr = VsUiShell.SetWaitCursor();
                }

                string content = File.ReadAllText(pszFilename);
                
                PnmlDocument document = new PnmlDocument();
                document.Load(content);

                PnmlPersister persister = new PnmlPersister();
                PetriNet petriNet = persister.Import(document);

                editorControl.Editor.LoadPetriNet(petriNet);

                isDirty = false;

                //Determine if the file is read only on the file system
                FileAttributes fileAttrs = File.GetAttributes(pszFilename);

                int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;

                //Set readonly if either the file is readonly for the user or on the file system
                if (0 == isReadOnly && 0 == fReadOnly)
                    SetReadOnly(false);
                else
                    SetReadOnly(true);


                // Notify to the property window that some of the selected objects are changed
                ITrackSelection track = TrackSelection;
                if (null != track)
                {
                    hr = track.OnSelectChange((ISelectionContainer)selContainer);
                    if (ErrorHandler.Failed(hr))
                        return hr;
                }

                // Hook up to file change notifications
                if (String.IsNullOrEmpty(fileName) || 0 != String.Compare(fileName, pszFilename, true, CultureInfo.CurrentCulture))
                {
                    fileName = pszFilename;
                    SetFileChangeNotification(pszFilename, true);

                    // Notify the load or reload
                    NotifyDocChanged();
                }
            }
            finally
            {
                loading = false;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines whether an object has changed since being saved to its current file
        /// </summary>
        /// <param name="pfIsDirty">true if the document has changed</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.IsDirty(out int pfIsDirty)
        {
            if (isDirty)
            {
                pfIsDirty = 1;
            }
            else
            {
                pfIsDirty = 0;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Save the contents of the textbox into the specified file. If doing the save on the same file, we need to
        /// suspend notifications for file changes during the save operation.
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name. If the pszFilename parameter is a null reference 
        /// we need to save using the current file
        /// </param>
        /// <param name="remember">Boolean value that indicates whether the pszFileName parameter is to be used 
        /// as the current working file.
        /// If remember != 0, pszFileName needs to be made the current file and the dirty flag needs to be cleared after the save.
        ///                   Also, file notifications need to be enabled for the new file and disabled for the old file 
        /// If remember == 0, this save operation is a Save a Copy As operation. In this case, 
        ///                   the current file is unchanged and dirty flag is not cleared
        /// </param>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the format in which 
        /// the file will be saved</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            int hr = VSConstants.S_OK;
            bool doingSaveOnSameFile = false;
            // If file is null or same --> SAVE
            if (pszFilename == null || pszFilename == fileName)
            {
                fRemember = 1;
                doingSaveOnSameFile = true;
            }

            //Suspend file change notifications for only Save since we don't have notifications setup
            //for SaveAs and SaveCopyAs (as they are different files)
            if (doingSaveOnSameFile)
                this.SuspendFileChangeNotification(pszFilename, 1);

            try
            {
                if (pszFilename != null)
                {
                    PnmlPersister persister = new PnmlPersister();
                    PersistentDocument document = persister.Export(editorControl.Editor.ExportToPetriNet());
                    File.WriteAllText(pszFilename, document.GetContent());
                }
            }
            catch (ArgumentException)
            {
                hr = VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                hr = VSConstants.E_FAIL;
            }
            finally
            {
                //restore the file change notifications
                if (doingSaveOnSameFile)
                    this.SuspendFileChangeNotification(pszFilename, 0);
            }

            if (VSConstants.E_FAIL == hr)
                return hr;

            //Save and Save as
            if (fRemember != 0)
            {
                //Save as
                if (null != pszFilename && !fileName.Equals(pszFilename))
                {
                    SetFileChangeNotification(fileName, false); //remove notification from old file
                    SetFileChangeNotification(pszFilename, true); //add notification for new file
                    fileName = pszFilename;     //cache the new file name
                }
                isDirty = false;
                SetReadOnly(false);             //set read only to false since you were successfully able
                //to save to the new file                                                    
            }

            ITrackSelection track = TrackSelection;
            if (null != track)
            {
                hr = track.OnSelectChange((ISelectionContainer)selContainer);
            }

            // Since all changes are now saved properly to disk, there's no need for a backup.
            backupObsolete = false;
            return hr;
        }

        #endregion


        #region IVsPersistDocData Members

        /// <summary>
        /// Used to determine if the document data has changed since the last time it was saved
        /// </summary>
        /// <param name="pfDirty">Will be set to 1 if the data has changed</param>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
        {
            return ((IPersistFileFormat)this).IsDirty(out pfDirty);
        }

        /// <summary>
        /// Saves the document data. Before actually saving the file, we first need to indicate to the environment
        /// that a file is about to be saved. This is done through the "SVsQueryEditQuerySave" service. We call the
        /// "QuerySaveFile" function on the service instance and then proceed depending on the result returned as follows:
        /// If result is QSR_SaveOK - We go ahead and save the file and the file is not read only at this point.
        /// If result is QSR_ForceSaveAs - We invoke the "Save As" functionality which will bring up the Save file name 
        ///                                dialog 
        /// If result is QSR_NoSave_Cancel - We cancel the save operation and indicate that the document could not be saved
        ///                                by setting the "pfSaveCanceled" flag
        /// If result is QSR_NoSave_Continue - Nothing to do here as the file need not be saved
        /// </summary>
        /// <param name="dwSave">Flags which specify the file save options:
        /// VSSAVE_Save        - Saves the current file to itself.
        /// VSSAVE_SaveAs      - Prompts the User for a filename and saves the file to the file specified.
        /// VSSAVE_SaveCopyAs  - Prompts the user for a filename and saves a copy of the file with a name specified.
        /// VSSAVE_SilentSave  - Saves the file without prompting for a name or confirmation.  
        /// </param>
        /// <param name="pbstrMkDocumentNew">Pointer to the path to the new document</param>
        /// <param name="pfSaveCanceled">value 1 if the document could not be saved</param>
        /// <returns></returns>
        int IVsPersistDocData.SaveDocData(Microsoft.VisualStudio.Shell.Interop.VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;
            int hr = VSConstants.S_OK;

            switch (dwSave)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                    {
                        IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                        // Call QueryEditQuerySave
                        uint result = 0;
                        hr = queryEditQuerySave.QuerySaveFile(
                                fileName,        // filename
                                0,    // flags
                                null,            // file attributes
                                out result);    // result
                        if (ErrorHandler.Failed(hr))
                            return hr;

                        // Process according to result from QuerySave
                        switch ((tagVSQuerySaveResult)result)
                        {
                            case tagVSQuerySaveResult.QSR_NoSave_Cancel:
                                // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                                // two tags have the same value.
                                pfSaveCanceled = ~0;
                                break;

                            case tagVSQuerySaveResult.QSR_SaveOK:
                                {
                                    // Call the shell to do the save for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_ForceSaveAs:
                                {
                                    // Call the shell to do the SaveAS for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, (IPersistFileFormat)this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_NoSave_Continue:
                                // In this case there is nothing to do.
                                break;

                            default:
                                throw new NotSupportedException("Unsupported result from QEQS");
                        }
                        break;
                    }
                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                    {
                        // Make sure the file name as the right extension
                        if (String.Compare(pnmlExtension, System.IO.Path.GetExtension(fileName), true, CultureInfo.CurrentCulture) != 0)
                        {
                            fileName += pnmlExtension;
                        }
                        // Call the shell to do the save for us
                        IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                        hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                        if (ErrorHandler.Failed(hr))
                            return hr;
                        break;
                    }
                default:
                    throw new ArgumentException("Unsupported Save flag");
            };

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the document data from the file specified
        /// </summary>
        /// <param name="pszMkDocument">Path to the document file which needs to be loaded</param>
        /// <returns>S_Ok if the method succeeds</returns>
        int IVsPersistDocData.LoadDocData(string pszMkDocument)
        {
            return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
        }

        /// <summary>
        /// Used to set the initial name for unsaved, newly created document data
        /// </summary>
        /// <param name="pszDocDataPath">String containing the path to the document. We need to ignore this parameter
        /// </param>
        /// <returns>S_OK if the mthod succeeds</returns>
        int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
        {
            return ((IPersistFileFormat)this).InitNew(pnmlFormat);
        }

        /// <summary>
        /// Returns the Guid of the editor factory that created the IVsPersistDocData object
        /// </summary>
        /// <param name="pClassID">Pointer to the class identifier of the editor type</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
        {
            return ((IPersistFileFormat)this).GetClassID(out pClassID);
        }

        /// <summary>
        /// Close the IVsPersistDocData object
        /// </summary>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines if it is possible to reload the document data
        /// </summary>
        /// <param name="pfReloadable">set to 1 if the document can be reloaded</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
        {
            // Allow file to be reloaded
            pfReloadable = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Renames the document data
        /// </summary>
        /// <param name="grfAttribs"></param>
        /// <param name="pHierNew"></param>
        /// <param name="itemidNew"></param>
        /// <param name="pszMkDocumentNew"></param>
        /// <returns></returns>
        int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            // TODO:  Add EditorPane.RenameDocData implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Reloads the document data
        /// </summary>
        /// <param name="grfFlags">Flag indicating whether to ignore the next file change when reloading the document data.
        /// This flag should not be set for us since we implement the "IVsDocDataFileChangeControl" interface in order to 
        /// indicate ignoring of file changes
        /// </param>
        /// <returns>S_OK if the mthod succeeds</returns>
        int IVsPersistDocData.ReloadDocData(uint grfFlags)
        {
            return ((IPersistFileFormat)this).Load(fileName, grfFlags, 0);
        }

        /// <summary>
        /// Called by the Running Document Table when it registers the document data. 
        /// </summary>
        /// <param name="docCookie">Handle for the document to be registered</param>
        /// <param name="pHierNew">Pointer to the IVsHierarchy interface</param>
        /// <param name="itemidNew">Item identifier of the document to be registered from VSITEM</param>
        /// <returns></returns>
        int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsFileChangeEvents Members

        /// <summary>
        /// Notify the editor of the changes made to one or more files
        /// </summary>
        /// <param name="cChanges">Number of files that have changed</param>
        /// <param name="rgpszFile">array of the files names that have changed</param>
        /// <param name="rggrfChange">Array of the flags indicating the type of changes</param>
        /// <returns></returns>
        int IVsFileChangeEvents.FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** Inside FilesChanged ****"));

            //check the different parameters
            if (0 == cChanges || null == rgpszFile || null == rggrfChange)
                return VSConstants.E_INVALIDARG;

            //ignore file changes if we are in that mode
            if (ignoreFileChangeLevel != 0)
                return VSConstants.S_OK;

            for (uint i = 0; i < cChanges; i++)
            {
                if (!String.IsNullOrEmpty(rgpszFile[i]) && String.Compare(rgpszFile[i], fileName, true, CultureInfo.CurrentCulture) == 0)
                {
                    // if the readonly state (file attributes) have changed we can immediately update
                    // the editor to match the new state (either readonly or not readonly) immediately
                    // without prompting the user.
                    if (0 != (rggrfChange[i] & (int)_VSFILECHANGEFLAGS.VSFILECHG_Attr))
                    {
                        FileAttributes fileAttrs = File.GetAttributes(fileName);
                        int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                        SetReadOnly(isReadOnly != 0);
                    }
                    // if it looks like the file contents have changed (either the size or the modified
                    // time has changed) then we need to prompt the user to see if we should reload the
                    // file. it is important to not syncronisly reload the file inside of this FilesChanged
                    // notification. first it is possible that there will be more than one FilesChanged 
                    // notification being sent (sometimes you get separate notifications for file attribute
                    // changing and file size/time changing). also it is the preferred UI style to not
                    // prompt the user until the user re-activates the environment application window.
                    // this is why we use a timer to delay prompting the user.
                    if (0 != (rggrfChange[i] & (int)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size)))
                    {
                        if (!fileChangedTimerSet)
                        {
                            FileChangeTrigger = new Timer();
                            fileChangedTimerSet = true;
                            FileChangeTrigger.Interval = 1000;
                            FileChangeTrigger.Tick += new EventHandler(this.OnFileChangeEvent);
                            FileChangeTrigger.Enabled = true;
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notify the editor of the changes made to a directory
        /// </summary>
        /// <param name="pszDirectory">Name of the directory that has changed</param>
        /// <returns></returns>
        int IVsFileChangeEvents.DirectoryChanged(string pszDirectory)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsDocDataFileChangeControl Members

        /// <summary>
        /// Used to determine whether changes to DocData in files should be ignored or not
        /// </summary>
        /// <param name="fIgnore">a non zero value indicates that the file changes should be ignored
        /// </param>
        /// <returns></returns>
        int IVsDocDataFileChangeControl.IgnoreFileChanges(int fIgnore)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside IgnoreFileChanges ****"));

            if (fIgnore != 0)
            {
                ignoreFileChangeLevel++;
            }
            else
            {
                if (ignoreFileChangeLevel > 0)
                    ignoreFileChangeLevel--;

                // We need to check here if our file has changed from "Read Only"
                // to "Read/Write" or vice versa while the ignore level was non-zero.
                // This may happen when a file is checked in or out under source
                // code control. We need to check here so we can update our caption.
                FileAttributes fileAttrs = File.GetAttributes(fileName);
                int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                SetReadOnly(isReadOnly != 0);
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region File Change Notification Helpers

        /// <summary>
        /// In this function we inform the shell when we wish to receive 
        /// events when our file is changed or we inform the shell when 
        /// we wish not to receive events anymore.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fStart">TRUE indicates advise, FALSE indicates unadvise.</param>
        /// <returns>Result of teh operation</returns>
        private int SetFileChangeNotification(string pszFileName, bool fStart)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SetFileChangeNotification ****"));

            int result = VSConstants.E_FAIL;

            //Get the File Change service
            if (null == vsFileChangeEx)
                vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            // Setup Notification if fStart is TRUE, Remove if fStart is FALSE.
            if (fStart)
            {
                if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                {
                    //Receive notifications if either the attributes of the file change or 
                    //if the size of the file changes or if the last modified time of the file changes
                    result = vsFileChangeEx.AdviseFileChange(pszFileName,
                        (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Attr | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time),
                        (IVsFileChangeEvents)this,
                        out vsFileChangeCookie);
                    if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                        return VSConstants.E_FAIL;
                }
            }
            else
            {
                if (vsFileChangeCookie != VSConstants.VSCOOKIE_NIL)
                {
                    result = vsFileChangeEx.UnadviseFileChange(vsFileChangeCookie);
                    vsFileChangeCookie = VSConstants.VSCOOKIE_NIL;
                }
            }
            return result;
        }

        /// <summary>
        /// In this function we suspend receiving file change events for
        /// a file or we reinstate a previously suspended file depending
        /// on the value of the given fSuspend flag.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fSuspend">TRUE indicates that the events needs to be suspended</param>
        /// <returns></returns>

        private int SuspendFileChangeNotification(string pszFileName, int fSuspend)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SuspendFileChangeNotification ****"));

            if (null == vsFileChangeEx)
                vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            if (0 == fSuspend)
            {
                // we are transitioning from suspended to non-suspended state - so force a
                // sync first to avoid asynchronous notifications of our own change
                if (vsFileChangeEx.SyncFile(pszFileName) == VSConstants.E_FAIL)
                    return VSConstants.E_FAIL;
            }

            //If we use the VSCOOKIE parameter to specify the file, then pszMkDocument parameter 
            //must be set to a null reference and vice versa 
            return vsFileChangeEx.IgnoreFile(vsFileChangeCookie, null, fSuspend);
        }
        #endregion

        #region IVsFileBackup Members

        /// <summary>
        /// This method is used to Persist the data to a single file. On a successful backup this 
        /// should clear up the backup dirty bit
        /// </summary>
        /// <param name="pszBackupFileName">Name of the file to persist</param>
        /// <returns>S_OK if the data can be successfully persisted.
        /// This should return STG_S_DATALOSS or STG_E_INVALIDCODEPAGE if there is no way to 
        /// persist to a file without data loss
        /// </returns>
        int IVsFileBackup.BackupFile(string pszBackupFileName)
        {
            try
            {
                //!!editorControl.RichTextBoxControl.SaveFile(pszBackupFileName);
                backupObsolete = false;
            }
            catch (ArgumentException)
            {
                return VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Used to set the backup dirty bit. This bit should be set when the object is modified 
        /// and cleared on calls to BackupFile and any Save method
        /// </summary>
        /// <param name="pbObsolete">the dirty bit to be set</param>
        /// <returns>returns 1 if the backup dirty bit is set, 0 otherwise</returns>
        int IVsFileBackup.IsBackupFileObsolete(out int pbObsolete)
        {
            if (backupObsolete)
                pbObsolete = 1;
            else
                pbObsolete = 0;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsToolboxUser Interface

        int IVsToolboxUser.IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
        {
            ToolboxItem item = toolboxService.DeserializeToolboxItem(pDO);
            string itemDataKey = PetriNetEditorToolboxProvider.ItemCommandNameProperty;

            if (item.Properties.Contains(itemDataKey) && item.Properties[itemDataKey] is PetriNetEditorToolboxItemData)
            {
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        int IVsToolboxUser.ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
        {
            ToolboxItem item = toolboxService.DeserializeToolboxItem(pDO);
            string itemDataKey = PetriNetEditorToolboxProvider.ItemCommandNameProperty;

            if (item.Properties.Contains(itemDataKey) && item.Properties[itemDataKey] is PetriNetEditorToolboxItemData)
            {
                string commandName = ((PetriNetEditorToolboxItemData)item.Properties[itemDataKey]).CommandName;
                Type widgetType = null;
                switch (commandName)
                {
                    case "Place":
                        widgetType = typeof(PlaceWidget);
                        break;
                    case "Transition":
                        widgetType = typeof(TransitionWidget);
                        break;
                    case "AndSplit":
                        widgetType = typeof(AndSplitWidget);
                        break;
                    case "AndJoin":
                        widgetType = typeof(AndJoinWidget);
                        break;
                    case "OrSplit":
                        widgetType = typeof(OrSplitWidget);
                        break;
                    case "OrJoin":
                        widgetType = typeof(OrJoinWidget);
                        break;
                }
                if (widgetType != null)
                {
                    editorControl.Editor.AddWidget(widgetType);
                    RefreshObjectSelection();
                }
            }
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        /// Used to ReadOnly property for the Rich TextBox and correspondingly update the editor caption
        /// </summary>
        /// <param name="_isFileReadOnly">Indicates whether the file loaded is Read Only or not</param>
        private void SetReadOnly(bool _isFileReadOnly)
        {
            //!!this.editorControl.RichTextBoxControl.ReadOnly = _isFileReadOnly;

            //update editor caption with "[Read Only]" or "" as necessary
            IVsWindowFrame frame = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
            string editorCaption = "";
            if (_isFileReadOnly)
                editorCaption = this.GetResourceString("@100");
            ErrorHandler.ThrowOnFailure(frame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, editorCaption));
            backupObsolete = true;
        }

        /// <summary>
        /// This event is triggered when one of the files loaded into the environment has changed outside of the
        /// editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChangeEvent(object sender, System.EventArgs e)
        {
            //Disable the timer
            FileChangeTrigger.Enabled = false;

            string message = this.GetResourceString("@101");    //get the message string from the resource
            IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            int result = 0;
            Guid tempGuid = Guid.Empty;
            if (VsUiShell != null)
            {
                //Show up a message box indicating that the file has changed outside of VS environment
                ErrorHandler.ThrowOnFailure(VsUiShell.ShowMessageBox(0, ref tempGuid, fileName, message, null, 0,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_QUERY, 0, out result));
            }
            //if the user selects "Yes", reload the current file
            if (result == (int)DialogResult.Yes)
            {
                ErrorHandler.ThrowOnFailure(((IVsPersistDocData)this).ReloadDocData(0));
            }

            fileChangedTimerSet = false;
        }

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = package.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        /// <summary>
        /// This function asks to the QueryEditQuerySave service if it is possible to
        /// edit the file.
        /// </summary>
        private bool CanEditFile()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** CanEditFile called ****"));

            // Check the status of the recursion guard
            if (gettingCheckoutStatus)
                return false;

            try
            {
                // Set the recursion guard
                gettingCheckoutStatus = true;

                // Get the QueryEditQuerySave service
                IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                // Now call the QueryEdit method to find the edit status of this file
                string[] documents = { this.fileName };
                uint result;
                uint outFlags;

                // Note that this function can popup a dialog to ask the user to checkout the file.
                // When this dialog is visible, it is possible to receive other request to change
                // the file and this is the reason for the recursion guard.
                int hr = queryEditQuerySave.QueryEditFiles(
                    0,              // Flags
                    1,              // Number of elements in the array
                    documents,      // Files to edit
                    null,           // Input flags
                    null,           // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                    out result,     // result of the checkout
                    out outFlags    // Additional flags
                );
                if (ErrorHandler.Succeeded(hr) && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                {
                    // In this case (and only in this case) we can return true from this function.
                    return true;
                }
            }

            finally
            {
                gettingCheckoutStatus = false;
            }
            return false;
        }

        /// <summary>
        /// This event is triggered when the control's GotFocus event is fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGotFocus(object sender, System.EventArgs e)
        {
            if (null == FNFStatusbarTrigger)
                FNFStatusbarTrigger = new Timer();

            FileChangeTrigger.Interval = 1000;
            FNFStatusbarTrigger.Tick += new EventHandler(this.OnSetStatusBar);
            FNFStatusbarTrigger.Start();
        }

        private void OnSetStatusBar(object sender, System.EventArgs e)
        {
            FNFStatusbarTrigger.Stop();
            ErrorHandler.ThrowOnFailure(((IVsStatusbarUser)this).SetInfo());
        }

        private void RefreshObjectSelection()
        {
            if (frame == null)
            {
                var shell = GetService(typeof(SVsUIShell)) as IVsUIShell;
                if (shell != null)
                {
                    var guidPropertyBrowser = new Guid(ToolWindowGuids.PropertyBrowser);
                    shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guidPropertyBrowser, out frame);
                }
            }
            if (frame != null)
            {
                frame.Show();
            }
            if (selContainer == null)
            {
                selContainer = new Microsoft.VisualStudio.Shell.SelectionContainer();
            }

            var mySelItems = new ArrayList();

            var selected = editorControl.Editor.SelectedWidgets;
            if (selected != null && selected.Count > 0)
            {
                mySelItems.Add(selected[0].Element);
            }

            selContainer.SelectableObjects = selContainer.SelectedObjects = mySelItems;

            ITrackSelection track = TrackSelection;
            if (null != track)
            {
                track.OnSelectChange((ISelectionContainer)selContainer);
            }
        }

        #region IVsStatusbarUser Members

        /// <summary>
        /// This is the IVsStatusBarUser function that will update our status bar.
        /// Note that the IDE calls this function only when our document window is
        /// initially activated.
        /// </summary>
        /// <returns> Hresult that represents success or failure.</returns>
        int IVsStatusbarUser.SetInfo()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Helper function that updates the insert mode displayed on the status bar.
        /// This is the text that is displayed in the right side of the status bar that
        /// will either say INS or OVR.
        /// </summary>
        /// <returns> Hresult that represents success or failure.</returns>
        int SetStatusBarInsertMode()
        {
            // Get the IVsStatusBar interface
            IVsStatusbar statusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
                return VSConstants.E_FAIL;

            // Set the insert mode based on our editorControl.richTextBoxCtrl.Overstrike value.  If 1 is passed
            // in then it will display OVR and if 0 is passed in it will display INS.
            object insertMode = 0;
            return statusBar.SetInsMode(ref insertMode);
        }

        /// <summary>
        /// This is an extra command handler that we will use to check when the insert
        /// key is pressed.  Note that even if we detect that the insert key is pressed
        /// we are not setting the handled property to true, so other event handlers will
        /// also see it.
        /// </summary>
        /// <param name="sender"> Not used.</param>
        /// <param name="e"> KeyEventArgs instance that we will use to get the key that was pressed.</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            //!!
            // If the key pressed is the insert key...
            if (e.KeyValue == 45)
            {
                // Toggle our stored insert value
                //this.editorControl.Overstrike = !this.editorControl.Overstrike;

                // Call the function to update the status bar insert mode
                SetStatusBarInsertMode();
            }
        }

        /// <summary>
        /// Helper function that updates the selection mode displayed on the status
        /// bar.  Right now we only support stream selection.
        /// </summary>
        /// <returns> Hresult that represents success or failure.</returns>
        int SetStatusBarSelectionMode()
        {
            // Get the IVsStatusBar interface.
            IVsStatusbar statusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
                return VSConstants.E_FAIL;

            // Set the selection mode.  Since we only support stream selection we will
            // always pass in zero here.  Passing in one would make "COL" show up
            // just to the left of the insert mode on the status bar.
            object selectionMode = 0;
            return statusBar.SetSelMode(ref selectionMode);
        }

        #endregion
    }
}
