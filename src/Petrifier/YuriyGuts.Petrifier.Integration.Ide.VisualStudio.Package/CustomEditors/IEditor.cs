using System.Runtime.InteropServices;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Package
{
    /// <summary>
    /// IEditor is the automation interface for EditorDocument.
    /// The implementation of the methods is just a wrapper over the rich
    /// edit control's object model.
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IEditor
    {
    }
}
