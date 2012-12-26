using System;
using System.IO;
using System.Windows.Forms;
using YuriyGuts.Petrifier.Core.PetriNets;
using YuriyGuts.Petrifier.Diagnostics.Verification;
using YuriyGuts.Petrifier.Persistence.Pnml;
using YuriyGuts.Petrifier.Runtime;
using YuriyGuts.Petrifier.Runtime.Hosting;

namespace YuriyGuts.Petrifier.TestApplication
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnImportPnmlIntoEditor_Click(object sender, EventArgs e)
        {
            if (dlgOpenPnml.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string fileName = dlgOpenPnml.FileName;
            var document = new PnmlDocument();
            document.Load(File.ReadAllText(fileName));
            var petriNet = new PnmlPersister().Import(document);

            petriNetEditor.LoadPetriNet(petriNet);
        }

        private void btnExportEditorContentToPnml_Click(object sender, EventArgs e)
        {
            if (dlgSavePnml.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string fileName = dlgSavePnml.FileName;
            var petriNet = petriNetEditor.ExportToPetriNet();
            var pnml = new PnmlPersister().Export(petriNet);

            File.WriteAllText(fileName, pnml.GetContent());
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            var petriNet = petriNetEditor.ExportToPetriNet();
            var verificationResult = new PetriNetSoundnessVerifier().Verify(new PetriNetVerificationContext { PetriNet = petriNet });
            var runtimeHost = new EmbeddedRuntimeHost(typeof(PetrifierRuntime));
            runtimeHost.ExecutePetriNetModule(new PetriNetModule(petriNet));
        }

        private void petriNetEditor_SelectedWidgetsChanged(object sender, EventArgs e)
        {
            if (petriNetEditor.SelectedWidgets.Count == 1)
            {
                propertyGrid.SelectedObject = petriNetEditor.SelectedWidgets[0].Element;
            }
        }
    }
}
