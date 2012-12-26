using YuriyGuts.Petrifier.Visualization.PetriNets;

namespace YuriyGuts.Petrifier.TestApplication
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnExecute = new System.Windows.Forms.Button();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.btnImportPnmlIntoEditor = new System.Windows.Forms.Button();
            this.btnExportEditorContentToPnml = new System.Windows.Forms.Button();
            this.editorSplitContainer = new System.Windows.Forms.SplitContainer();
            this.petriNetEditor = new YuriyGuts.Petrifier.Visualization.PetriNets.PetriNetEditor();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.dlgOpenPnml = new System.Windows.Forms.OpenFileDialog();
            this.dlgSavePnml = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.editorSplitContainer)).BeginInit();
            this.editorSplitContainer.Panel1.SuspendLayout();
            this.editorSplitContainer.Panel2.SuspendLayout();
            this.editorSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(279, 0);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(135, 40);
            this.btnExecute.TabIndex = 0;
            this.btnExecute.Text = "Execute Petri Net";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.mainSplitContainer.Location = new System.Drawing.Point(10, 10);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.btnImportPnmlIntoEditor);
            this.mainSplitContainer.Panel1.Controls.Add(this.btnExportEditorContentToPnml);
            this.mainSplitContainer.Panel1.Controls.Add(this.btnExecute);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.editorSplitContainer);
            this.mainSplitContainer.Size = new System.Drawing.Size(864, 542);
            this.mainSplitContainer.SplitterDistance = 43;
            this.mainSplitContainer.SplitterWidth = 10;
            this.mainSplitContainer.TabIndex = 1;
            // 
            // btnImportPnmlIntoEditor
            // 
            this.btnImportPnmlIntoEditor.Location = new System.Drawing.Point(0, 0);
            this.btnImportPnmlIntoEditor.Name = "btnImportPnmlIntoEditor";
            this.btnImportPnmlIntoEditor.Size = new System.Drawing.Size(136, 40);
            this.btnImportPnmlIntoEditor.TabIndex = 2;
            this.btnImportPnmlIntoEditor.Text = "Open PNML...";
            this.btnImportPnmlIntoEditor.UseVisualStyleBackColor = true;
            this.btnImportPnmlIntoEditor.Click += new System.EventHandler(this.btnImportPnmlIntoEditor_Click);
            // 
            // btnExportEditorContentToPnml
            // 
            this.btnExportEditorContentToPnml.Location = new System.Drawing.Point(142, 0);
            this.btnExportEditorContentToPnml.Name = "btnExportEditorContentToPnml";
            this.btnExportEditorContentToPnml.Size = new System.Drawing.Size(131, 40);
            this.btnExportEditorContentToPnml.TabIndex = 2;
            this.btnExportEditorContentToPnml.Text = "Save as PNML...";
            this.btnExportEditorContentToPnml.UseVisualStyleBackColor = true;
            this.btnExportEditorContentToPnml.Click += new System.EventHandler(this.btnExportEditorContentToPnml_Click);
            // 
            // editorSplitContainer
            // 
            this.editorSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.editorSplitContainer.Name = "editorSplitContainer";
            // 
            // editorSplitContainer.Panel1
            // 
            this.editorSplitContainer.Panel1.Controls.Add(this.petriNetEditor);
            // 
            // editorSplitContainer.Panel2
            // 
            this.editorSplitContainer.Panel2.Controls.Add(this.propertyGrid);
            this.editorSplitContainer.Size = new System.Drawing.Size(864, 489);
            this.editorSplitContainer.SplitterDistance = 560;
            this.editorSplitContainer.SplitterWidth = 10;
            this.editorSplitContainer.TabIndex = 1;
            // 
            // petriNetEditor
            // 
            this.petriNetEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.petriNetEditor.Location = new System.Drawing.Point(0, 0);
            this.petriNetEditor.Name = "petriNetEditor";
            this.petriNetEditor.Size = new System.Drawing.Size(560, 489);
            this.petriNetEditor.TabIndex = 0;
            this.petriNetEditor.SelectedWidgetsChanged += new System.EventHandler(this.petriNetEditor_SelectedWidgetsChanged);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(294, 489);
            this.propertyGrid.TabIndex = 0;
            // 
            // dlgOpenPnml
            // 
            this.dlgOpenPnml.DefaultExt = "pnml";
            this.dlgOpenPnml.Filter = "Petri Net Markup Language Files (*.pnml)|*.pnml|All files|*.*";
            // 
            // dlgSavePnml
            // 
            this.dlgSavePnml.DefaultExt = "pnml";
            this.dlgSavePnml.Filter = "Petri Net Markup Language Files (*.pnml)|*.pnml|All files|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 562);
            this.Controls.Add(this.mainSplitContainer);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "Petrifier Test Application";
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.editorSplitContainer.Panel1.ResumeLayout(false);
            this.editorSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.editorSplitContainer)).EndInit();
            this.editorSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private PetriNetEditor petriNetEditor;
        private System.Windows.Forms.Button btnExportEditorContentToPnml;
        private System.Windows.Forms.Button btnImportPnmlIntoEditor;
        private System.Windows.Forms.SplitContainer editorSplitContainer;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.OpenFileDialog dlgOpenPnml;
        private System.Windows.Forms.SaveFileDialog dlgSavePnml;
    }
}

