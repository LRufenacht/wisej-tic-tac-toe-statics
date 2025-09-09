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
			this.button1 = new Wisej.Web.Button();
			SuspendLayout();
			// 
			// button1
			// 
			this.button1.CssStyle = "transition: margin 0.4s ease, flex-grow 0.4s ease;";
			this.button1.Focusable = false;
			this.button1.Location = new System.Drawing.Point(93, 65);
			this.button1.Movable = true;
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(100, 37);
			this.button1.TabIndex = 0;
			this.button1.Text = "button1";
			this.button1.MouseDown += button1_MouseDown;
			this.button1.MouseMove += button1_MouseMove;
			this.button1.MouseUp += button1_MouseUp;
			// 
			// Page1
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
			AutoScaleMode = Web.AutoScaleMode.Font;
			Controls.Add(this.button1);
			Name = "Page1";
			Size = new System.Drawing.Size(2681, 789);
			Load += Page1_Load;
			Disposed += Page1_Disposed;
			ResumeLayout(false);

		}

		#endregion

		private Web.Button button1;
	}
}
