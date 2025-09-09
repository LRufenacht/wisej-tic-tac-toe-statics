using System;
using System.Drawing;
using Wisej.Web;

namespace Wisej.StaticsExample
{
    public partial class Page1 : Page
    {
        private Button[] _buttons = new Button[9];

        public Page1()
        {
            InitializeComponent();
        }

        private void Page1_Load(object sender, EventArgs e)
        {
            CreateBoard();

            TicTacToeGame.MoveMade += TicTacToeGame_MoveMade;
            TicTacToeGame.GameReset += TicTacToeGame_GameReset;
            TicTacToeGame.GameEnded += TicTacToeGame_GameEnded;

            RenderBoard();
        }

        private void CreateBoard()
        {
            int index = 0;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var btn = new Button();
                    btn.Dock = DockStyle.Fill;
                    btn.Font = new Font(this.Font.FontFamily, 24f, FontStyle.Bold);
                    btn.Tag = new Point(r, c);
                    btn.Click += Cell_Click;
                    this.tableLayoutPanel1.Controls.Add(btn, c, r);
                    _buttons[index++] = btn;
                }
            }
        }

        private void RenderBoard()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var index = r * 3 + c;
                    var value = TicTacToeGame.Board[r, c];
                    _buttons[index].Text = value == '\0' ? "" : value.ToString();
                    _buttons[index].Enabled = !TicTacToeGame.GameOver && value == '\0';
                }
            }
            this.labelStatus.Text = TicTacToeGame.Message;
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var pt = (Point)btn.Tag;
            TicTacToeGame.MakeMove(pt.X, pt.Y);
        }

        private void TicTacToeGame_MoveMade(object sender, MoveEventArgs e)
        {
            Application.Update(this, () =>
            {
                var index = e.Row * 3 + e.Col;
                _buttons[index].Text = e.Player.ToString();
                _buttons[index].Enabled = false;
                this.labelStatus.Text = TicTacToeGame.Message;
                if (TicTacToeGame.GameOver)
                    DisableBoard();
            });
        }

        private void TicTacToeGame_GameReset(object sender, EventArgs e)
        {
            Application.Update(this, () =>
            {
                foreach (var btn in _buttons)
                {
                    btn.Text = "";
                    btn.Enabled = true;
                }
                this.labelStatus.Text = TicTacToeGame.Message;
            });
        }

        private void TicTacToeGame_GameEnded(object sender, string e)
        {
            Application.Update(this, () =>
            {
                this.labelStatus.Text = e;
                DisableBoard();
            });
        }

        private void DisableBoard()
        {
            foreach (var btn in _buttons)
                btn.Enabled = false;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            TicTacToeGame.Reset();
        }

        private void Page1_Disposed(object sender, EventArgs e)
        {
            TicTacToeGame.MoveMade -= TicTacToeGame_MoveMade;
            TicTacToeGame.GameReset -= TicTacToeGame_GameReset;
            TicTacToeGame.GameEnded -= TicTacToeGame_GameEnded;
        }
    }
}
