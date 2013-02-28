namespace NuoDb.VisualStudio.DataTools.Editors
{
    partial class SQLEditor
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.CommandWindow = new NuoDb.VisualStudio.DataTools.Editors.CodeWindow(this);
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.connectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.ResultsWindow = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.CommandWindow);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.ResultsWindow);
            this.splitContainer.Size = new System.Drawing.Size(820, 457);
            this.splitContainer.SplitterDistance = 273;
            this.splitContainer.TabIndex = 0;
            // 
            // CommandWindow
            // 
            this.CommandWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommandWindow.Location = new System.Drawing.Point(0, 0);
            this.CommandWindow.Name = "CommandWindow";
            this.CommandWindow.Size = new System.Drawing.Size(820, 273);
            this.CommandWindow.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 158);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(820, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip";
            // 
            // connectionStatus
            // 
            this.connectionStatus.Name = "connectionStatus";
            this.connectionStatus.Size = new System.Drawing.Size(0, 17);
            this.connectionStatus.Text = "Disconnected";
            // 
            // ResultsWindow
            // 
            this.ResultsWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResultsWindow.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResultsWindow.Location = new System.Drawing.Point(0, 0);
            this.ResultsWindow.Name = "ResultsWindow";
            this.ResultsWindow.Size = new System.Drawing.Size(820, 180);
            this.ResultsWindow.TabIndex = 0;
            this.ResultsWindow.Text = "";
            // 
            // SQLEditor
            // 
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusStrip);
            this.Name = "SQLEditor";
            this.Size = new System.Drawing.Size(820, 457);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.RichTextBox ResultsWindow;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel connectionStatus;
        private CodeWindow CommandWindow;

    }
}
