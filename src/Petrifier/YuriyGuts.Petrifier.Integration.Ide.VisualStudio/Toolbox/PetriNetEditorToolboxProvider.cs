using System.Collections.Generic;
using System.Drawing.Design;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Toolbox
{
    public class PetriNetEditorToolboxProvider
    {
        public const string ToolboxCategoryName = "Petri Nets";
        public const string ItemCommandNameProperty = "CommandName";

        public List<ToolboxItem> GetToolboxItemList()
        {
            List<dynamic> itemMetadataList = new List<dynamic>
            {
                new { DisplayName = "Place", Bitmap = Resources.Place, CommandName = "Place" },
                new { DisplayName = "Transition", Bitmap = Resources.Transition, CommandName = "Transition" },
                new { DisplayName = "AND-split", Bitmap = Resources.AndSplit, CommandName = "AndSplit" },
                new { DisplayName = "OR-split", Bitmap = Resources.OrSplit, CommandName = "OrSplit" },
                new { DisplayName = "AND-join", Bitmap = Resources.AndJoin, CommandName = "AndJoin" },
                new { DisplayName = "OR-join", Bitmap = Resources.OrJoin, CommandName = "OrJoin" },
                new { DisplayName = "Subprocess", Bitmap = Resources.Subprocess, CommandName = "Subprocess" },
            };

            List<ToolboxItem> result = new List<ToolboxItem>();
            foreach (dynamic metadata in itemMetadataList)
            {
                ToolboxItem toolboxItem = new ToolboxItem();
                toolboxItem.Properties.Add(
                    ItemCommandNameProperty,
                    new PetriNetEditorToolboxItemData
                    {
                        CommandName = metadata.CommandName
                    });

                toolboxItem.DisplayName = metadata.DisplayName;
                toolboxItem.Bitmap = metadata.Bitmap;

                result.Add(toolboxItem);
            }

            return result;
        }
    }
}
