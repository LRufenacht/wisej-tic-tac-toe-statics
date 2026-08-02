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
            this.marioGame = new Wisej.StaticsExample.MarioGame();
            this.labelStatus = new Wisej.Web.Label();
            this.buttonReset = new Wisej.Web.Button();
            this.SuspendLayout();
            //
            // marioGame
            //
            this.marioGame.Location = new System.Drawing.Point(20, 20);
            this.marioGame.Name = "marioGame";
            this.marioGame.Size = new System.Drawing.Size(800, 400);
            this.marioGame.TabIndex = 0;
            //
            // labelStatus
            //
            this.labelStatus.Location = new System.Drawing.Point(20, 430);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(680, 30);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "";
            //
            // buttonReset
            //
            this.buttonReset.Location = new System.Drawing.Point(720, 430);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(100, 30);
            this.buttonReset.TabIndex = 2;
            this.buttonReset.Text = "Reset";
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            //
            // Page1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
            this.AutoScaleMode = Wisej.Web.AutoScaleMode.Font;
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.marioGame);
            this.Name = "Page1";
            this.Size = new System.Drawing.Size(840, 480);
            this.Text = "Super Wisej Bros.";
            this.Load += new System.EventHandler(this.Page1_Load);
            this.Disposed += new System.EventHandler(this.Page1_Disposed);
            this.ResumeLayout(false);
        }

        #endregion

        private Wisej.StaticsExample.MarioGame marioGame;
        private Wisej.Web.Label labelStatus;
        private Wisej.Web.Button buttonReset;
    }
}
