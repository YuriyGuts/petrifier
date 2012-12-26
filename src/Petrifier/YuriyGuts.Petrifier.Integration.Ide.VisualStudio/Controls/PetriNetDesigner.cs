using System;
using System.Windows.Forms;
using YuriyGuts.Petrifier.Visualization.PetriNets;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Controls
{
    public partial class PetriNetDesigner : UserControl
    {
        public PetriNetEditor Editor
        {
            get { return petriNetEditor; }
        }

        public PetriNetDesigner()
        {
            InitializeComponent();
        }

        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged(EventArgs args)
        {
            var handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        private void petriNetEditor_SelectedWidgetsChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(EventArgs.Empty);
        }
    }
}
