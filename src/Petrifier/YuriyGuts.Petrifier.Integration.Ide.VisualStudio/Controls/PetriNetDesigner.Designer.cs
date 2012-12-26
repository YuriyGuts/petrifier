using YuriyGuts.Petrifier.Visualization.PetriNets;

namespace YuriyGuts.Petrifier.Integration.Ide.VisualStudio.Controls
{
    partial class PetriNetDesigner
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.petriNetEditor = new YuriyGuts.Petrifier.Visualization.PetriNets.PetriNetEditor();
            this.SuspendLayout();
            // 
            // petriNetEditor
            // 
            this.petriNetEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.petriNetEditor.Location = new System.Drawing.Point(0, 0);
            this.petriNetEditor.Name = "petriNetEditor";
            this.petriNetEditor.Size = new System.Drawing.Size(297, 150);
            this.petriNetEditor.TabIndex = 0;
            this.petriNetEditor.SelectedWidgetsChanged += new System.EventHandler(this.petriNetEditor_SelectedWidgetsChanged);
            // 
            // PetriNetDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.petriNetEditor);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.Name = "PetriNetDesigner";
            this.Size = new System.Drawing.Size(297, 150);
            this.ResumeLayout(false);

        }

        #endregion

        private PetriNetEditor petriNetEditor;
    }
}
