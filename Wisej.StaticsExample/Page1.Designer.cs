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
            this.components = new System.ComponentModel.Container();
            this.gameArea = new Wisej.Web.Panel();
            this.scoreLabel = new Wisej.Web.Label();
            this.buttonReset = new Wisej.Web.Button();
            this.hintLabel = new Wisej.Web.Label();
            this.gameTimer = new Wisej.Web.Timer(this.components);
            this.SuspendLayout();
            //
            // gameArea
            //
            this.gameArea.BackColor = System.Drawing.Color.SkyBlue;
            this.gameArea.Focusable = true;
            this.gameArea.Location = new System.Drawing.Point(20, 65);
            this.gameArea.Name = "gameArea";
            this.gameArea.Size = new System.Drawing.Size(760, 380);
            this.gameArea.TabIndex = 0;
            this.gameArea.TabStop = true;
            //
            // scoreLabel
            //
            this.scoreLabel.Location = new System.Drawing.Point(20, 20);
            this.scoreLabel.Name = "scoreLabel";
            this.scoreLabel.Size = new System.Drawing.Size(180, 30);
            this.scoreLabel.TabIndex = 1;
            this.scoreLabel.Text = "Coins: 0";
            //
            // buttonReset
            //
            this.buttonReset.Location = new System.Drawing.Point(680, 20);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(100, 30);
            this.buttonReset.TabIndex = 2;
            this.buttonReset.Text = "Reset";
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            //
            // hintLabel
            //
            this.hintLabel.ForeColor = System.Drawing.Color.DimGray;
            this.hintLabel.Location = new System.Drawing.Point(20, 455);
            this.hintLabel.Name = "hintLabel";
            this.hintLabel.Size = new System.Drawing.Size(760, 20);
            this.hintLabel.TabIndex = 3;
            this.hintLabel.Text = "Click the sky, then use ← → to move and Space / ↑ to jump. Collect coins, avoid the goomba.";
            //
            // gameTimer
            //
            this.gameTimer.Enabled = false;
            this.gameTimer.Interval = 40;
            this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
            //
            // Page1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
            this.AutoScaleMode = Wisej.Web.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.scoreLabel);
            this.Controls.Add(this.hintLabel);
            this.Controls.Add(this.gameArea);
            this.Name = "Page1";
            this.Size = new System.Drawing.Size(800, 485);
            this.Load += new System.EventHandler(this.Page1_Load);
            this.ResumeLayout(false);
        }

        #endregion

        private Wisej.Web.Panel gameArea;
        private Wisej.Web.Label scoreLabel;
        private Wisej.Web.Button buttonReset;
        private Wisej.Web.Label hintLabel;
        private Wisej.Web.Timer gameTimer;
    }
}
