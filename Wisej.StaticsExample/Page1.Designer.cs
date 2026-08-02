namespace Wisej.StaticsExample
{
    partial class Page1
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

        #region Wisej.NET Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gameCanvas = new Wisej.StaticsExample.MarioGame();
            this.bottomBar = new Wisej.Web.Panel();
            this.labelStatus = new Wisej.Web.Label();
            this.buttonReset = new Wisej.Web.Button();
            this.bottomBar.SuspendLayout();
            this.SuspendLayout();
            //
            // gameCanvas
            //
            this.gameCanvas.Dock = Wisej.Web.DockStyle.Fill;
            this.gameCanvas.Name = "gameCanvas";
            this.gameCanvas.TabIndex = 0;
            //
            // bottomBar
            //
            this.bottomBar.Controls.Add(this.labelStatus);
            this.bottomBar.Controls.Add(this.buttonReset);
            this.bottomBar.Dock = Wisej.Web.DockStyle.Bottom;
            this.bottomBar.Name = "bottomBar";
            this.bottomBar.Size = new System.Drawing.Size(800, 44);
            this.bottomBar.TabIndex = 1;
            //
            // labelStatus
            //
            this.labelStatus.Location = new System.Drawing.Point(12, 12);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(600, 20);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "Score: 0";
            //
            // buttonReset
            //
            this.buttonReset.Anchor = ((Wisej.Web.AnchorStyles)(((Wisej.Web.AnchorStyles.Top | Wisej.Web.AnchorStyles.Right))));
            this.buttonReset.Location = new System.Drawing.Point(688, 8);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(100, 28);
            this.buttonReset.TabIndex = 1;
            this.buttonReset.Text = "Reset";
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            //
            // Page1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
            this.AutoScaleMode = Wisej.Web.AutoScaleMode.Font;
            this.Controls.Add(this.gameCanvas);
            this.Controls.Add(this.bottomBar);
            this.Name = "Page1";
            this.Size = new System.Drawing.Size(800, 500);
            this.Text = "Mini Mario";
            this.Load += new System.EventHandler(this.Page1_Load);
            this.Disposed += new System.EventHandler(this.Page1_Disposed);
            this.bottomBar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Wisej.StaticsExample.MarioGame gameCanvas;
        private Wisej.Web.Panel bottomBar;
        private Wisej.Web.Label labelStatus;
        private Wisej.Web.Button buttonReset;
    }
}
