namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    partial class PetriNetEditor
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
            this.imgCanvas = new System.Windows.Forms.PictureBox();
            this.tlbEditorActions = new System.Windows.Forms.ToolStrip();
            this.lblActions = new System.Windows.Forms.ToolStripLabel();
            this.btnConnect = new System.Windows.Forms.ToolStripButton();
            this.btnDisconnect = new System.Windows.Forms.ToolStripButton();
            this.btnInverseConnection = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.tlbSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.lblTools = new System.Windows.Forms.ToolStripLabel();
            this.btnPointer = new System.Windows.Forms.ToolStripButton();
            this.btnPlace = new System.Windows.Forms.ToolStripButton();
            this.btnTransition = new System.Windows.Forms.ToolStripButton();
            this.btnAndSplit = new System.Windows.Forms.ToolStripButton();
            this.btnOrSplit = new System.Windows.Forms.ToolStripButton();
            this.btnAndJoin = new System.Windows.Forms.ToolStripButton();
            this.btnOrJoin = new System.Windows.Forms.ToolStripButton();
            this.btnSubprocess = new System.Windows.Forms.ToolStripButton();
            this.focusContainer = new System.Windows.Forms.Button();
            this.pnlCanvasContainer = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.imgCanvas)).BeginInit();
            this.tlbEditorActions.SuspendLayout();
            this.pnlCanvasContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgCanvas
            // 
            this.imgCanvas.BackColor = System.Drawing.Color.White;
            this.imgCanvas.ErrorImage = null;
            this.imgCanvas.InitialImage = null;
            this.imgCanvas.Location = new System.Drawing.Point(0, 0);
            this.imgCanvas.Name = "imgCanvas";
            this.imgCanvas.Size = new System.Drawing.Size(552, 444);
            this.imgCanvas.TabIndex = 0;
            this.imgCanvas.TabStop = false;
            this.imgCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.imgCanvas_Paint);
            this.imgCanvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imgCanvas_MouseDown);
            this.imgCanvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imgCanvas_MouseMove);
            this.imgCanvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.imgCanvas_MouseUp);
            // 
            // tlbEditorActions
            // 
            this.tlbEditorActions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlbEditorActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblActions,
            this.btnConnect,
            this.btnDisconnect,
            this.btnInverseConnection,
            this.btnDelete,
            this.tlbSeparator1,
            this.lblTools,
            this.btnPointer,
            this.btnPlace,
            this.btnTransition,
            this.btnAndSplit,
            this.btnOrSplit,
            this.btnAndJoin,
            this.btnOrJoin,
            this.btnSubprocess});
            this.tlbEditorActions.Location = new System.Drawing.Point(0, 0);
            this.tlbEditorActions.Name = "tlbEditorActions";
            this.tlbEditorActions.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.tlbEditorActions.Size = new System.Drawing.Size(552, 25);
            this.tlbEditorActions.TabIndex = 1;
            this.tlbEditorActions.Text = "toolStrip1";
            // 
            // lblActions
            // 
            this.lblActions.Name = "lblActions";
            this.lblActions.Size = new System.Drawing.Size(50, 22);
            this.lblActions.Text = "Actions:";
            // 
            // btnConnect
            // 
            this.btnConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnConnect.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.Connect;
            this.btnConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(23, 22);
            this.btnConnect.Text = "Connect Elements";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDisconnect.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.Disconnect;
            this.btnDisconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(23, 22);
            this.btnDisconnect.Text = "Disconnect Elements";
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnInverseConnection
            // 
            this.btnInverseConnection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnInverseConnection.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.ConnectInverse;
            this.btnInverseConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnInverseConnection.Name = "btnInverseConnection";
            this.btnInverseConnection.Size = new System.Drawing.Size(23, 22);
            this.btnInverseConnection.Text = "Invert Connection";
            this.btnInverseConnection.Click += new System.EventHandler(this.btnInverseConnection_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDelete.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.Eraser;
            this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(23, 22);
            this.btnDelete.Text = "Delete Elements";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // tlbSeparator1
            // 
            this.tlbSeparator1.Name = "tlbSeparator1";
            this.tlbSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // lblTools
            // 
            this.lblTools.Name = "lblTools";
            this.lblTools.Size = new System.Drawing.Size(39, 22);
            this.lblTools.Text = "Tools:";
            // 
            // btnPointer
            // 
            this.btnPointer.CheckOnClick = true;
            this.btnPointer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPointer.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.Arrow;
            this.btnPointer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPointer.Name = "btnPointer";
            this.btnPointer.Size = new System.Drawing.Size(23, 22);
            this.btnPointer.Text = "Pointer";
            this.btnPointer.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // btnPlace
            // 
            this.btnPlace.CheckOnClick = true;
            this.btnPlace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPlace.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.Place;
            this.btnPlace.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPlace.Name = "btnPlace";
            this.btnPlace.Size = new System.Drawing.Size(23, 22);
            this.btnPlace.Text = "Place";
            this.btnPlace.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // btnTransition
            // 
            this.btnTransition.CheckOnClick = true;
            this.btnTransition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnTransition.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.Transition;
            this.btnTransition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTransition.Name = "btnTransition";
            this.btnTransition.Size = new System.Drawing.Size(23, 22);
            this.btnTransition.Text = "Transition";
            this.btnTransition.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // btnAndSplit
            // 
            this.btnAndSplit.CheckOnClick = true;
            this.btnAndSplit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAndSplit.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.AndSplit;
            this.btnAndSplit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAndSplit.Name = "btnAndSplit";
            this.btnAndSplit.Size = new System.Drawing.Size(23, 22);
            this.btnAndSplit.Text = "AND-Split";
            this.btnAndSplit.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // btnOrSplit
            // 
            this.btnOrSplit.CheckOnClick = true;
            this.btnOrSplit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOrSplit.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.OrSplit;
            this.btnOrSplit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOrSplit.Name = "btnOrSplit";
            this.btnOrSplit.Size = new System.Drawing.Size(23, 22);
            this.btnOrSplit.Text = "OR-Split";
            this.btnOrSplit.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // btnAndJoin
            // 
            this.btnAndJoin.CheckOnClick = true;
            this.btnAndJoin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAndJoin.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.AndJoin;
            this.btnAndJoin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAndJoin.Name = "btnAndJoin";
            this.btnAndJoin.Size = new System.Drawing.Size(23, 22);
            this.btnAndJoin.Text = "AND-Join";
            this.btnAndJoin.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // btnOrJoin
            // 
            this.btnOrJoin.CheckOnClick = true;
            this.btnOrJoin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOrJoin.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.OrJoin;
            this.btnOrJoin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOrJoin.Name = "btnOrJoin";
            this.btnOrJoin.Size = new System.Drawing.Size(23, 22);
            this.btnOrJoin.Text = "OR-Join";
            this.btnOrJoin.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // btnSubprocess
            // 
            this.btnSubprocess.CheckOnClick = true;
            this.btnSubprocess.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSubprocess.Image = global::YuriyGuts.Petrifier.Visualization.Properties.Resources.Subprocess;
            this.btnSubprocess.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSubprocess.Name = "btnSubprocess";
            this.btnSubprocess.Size = new System.Drawing.Size(23, 22);
            this.btnSubprocess.Text = "Subprocess";
            this.btnSubprocess.Click += new System.EventHandler(this.HandleToolSelected);
            // 
            // focusContainer
            // 
            this.focusContainer.Location = new System.Drawing.Point(1, 1);
            this.focusContainer.Name = "focusContainer";
            this.focusContainer.Size = new System.Drawing.Size(1, 1);
            this.focusContainer.TabIndex = 2;
            this.focusContainer.Text = "button1";
            this.focusContainer.UseVisualStyleBackColor = true;
            this.focusContainer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.focusContainer_KeyDown);
            // 
            // pnlCanvasContainer
            // 
            this.pnlCanvasContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCanvasContainer.AutoScroll = true;
            this.pnlCanvasContainer.Controls.Add(this.imgCanvas);
            this.pnlCanvasContainer.Location = new System.Drawing.Point(0, 25);
            this.pnlCanvasContainer.Name = "pnlCanvasContainer";
            this.pnlCanvasContainer.Size = new System.Drawing.Size(552, 444);
            this.pnlCanvasContainer.TabIndex = 3;
            // 
            // PetriNetEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlCanvasContainer);
            this.Controls.Add(this.tlbEditorActions);
            this.Controls.Add(this.focusContainer);
            this.Name = "PetriNetEditor";
            this.Size = new System.Drawing.Size(552, 469);
            this.Resize += new System.EventHandler(this.PetriNetEditor_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.imgCanvas)).EndInit();
            this.tlbEditorActions.ResumeLayout(false);
            this.tlbEditorActions.PerformLayout();
            this.pnlCanvasContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imgCanvas;
        private System.Windows.Forms.ToolStrip tlbEditorActions;
        private System.Windows.Forms.ToolStripLabel lblActions;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripButton btnConnect;
        private System.Windows.Forms.ToolStripButton btnDisconnect;
        private System.Windows.Forms.ToolStripLabel lblTools;
        private System.Windows.Forms.ToolStripButton btnPointer;
        private System.Windows.Forms.ToolStripButton btnPlace;
        private System.Windows.Forms.ToolStripButton btnTransition;
        private System.Windows.Forms.ToolStripButton btnAndSplit;
        private System.Windows.Forms.ToolStripButton btnOrSplit;
        private System.Windows.Forms.ToolStripButton btnAndJoin;
        private System.Windows.Forms.ToolStripButton btnOrJoin;
        private System.Windows.Forms.ToolStripButton btnSubprocess;
        private System.Windows.Forms.ToolStripSeparator tlbSeparator1;
        private System.Windows.Forms.ToolStripButton btnInverseConnection;
        private System.Windows.Forms.Button focusContainer;
        private System.Windows.Forms.Panel pnlCanvasContainer;
    }
}
