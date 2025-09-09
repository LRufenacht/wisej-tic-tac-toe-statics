using System;
using System.Drawing;
using Wisej.Web;

namespace Wisej.StaticsExample
{
	public partial class Page1 : Page
	{
		private static event EventHandler<bool> ButtonMoving;

		private static event EventHandler<Point> ButtonMoved;

		public Page1()
		{
			InitializeComponent();
		}

		private void Page1_Load(object sender, EventArgs e)
		{
			ButtonMoved += Page1_ButtonMoved;
			ButtonMoving += Page1_ButtonMoving;
		}

		private void Page1_ButtonMoving(object sender, bool e)
		{
			Application.Update(this, () =>
			{
				// if it's the sender, ignore.
				if (this._selected)
					return;

				this.button1.BackColor = e ? Color.Orange : Color.FromName("@button");
			});
		}

		private void Page1_ButtonMoved(object sender, Point e)
		{
			Application.Update(this, () =>
			{
				if (this._selected)
					return;

				this.button1.Location = new Point(e.X, e.Y);
			});
		}

		private void Page1_Disposed(object sender, EventArgs e)
		{
			ButtonMoved -= Page1_ButtonMoved;
		}

		private void button1_MouseDown(object sender, MouseEventArgs e)
		{
			this._selected = true;
			this.button1.BackColor = Color.Red;

			ButtonMoving?.Invoke(this, true);
		}
		private bool _selected;

		private void button1_MouseUp(object sender, MouseEventArgs e)
		{
			this._selected = false;
			this.button1.BackColor = Color.FromName("@button");

			ButtonMoving?.Invoke(this, false);
		}

		private void button1_MouseMove(object sender, MouseEventArgs e)
		{
			if (this._selected)
			{
				// cursor position relative to the button.
				var cursor = e.Location;

				// cursor position relative to the page.
				var page = this.button1.PointToScreen(cursor);

				ButtonMoved?.Invoke(this, e.Location);
			}
		}
	}
}
  