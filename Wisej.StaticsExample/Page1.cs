using System;
using Wisej.Web;

namespace Wisej.StaticsExample
{
    public partial class Page1 : Page
    {
        private int _bestScore;

        public Page1()
        {
            InitializeComponent();
        }

        private void Page1_Load(object sender, EventArgs e)
        {
            this.marioGame.ScoreChanged += MarioGame_ScoreChanged;
            this.marioGame.GameOver += MarioGame_GameOver;

            UpdateStatus(0, 3, null);
        }

        private void MarioGame_ScoreChanged(object sender, MarioScoreEventArgs e)
        {
            Application.Update(this, () => UpdateStatus(e.Score, e.Lives, null));
        }

        private void MarioGame_GameOver(object sender, MarioGameOverEventArgs e)
        {
            if (e.Score > _bestScore)
                _bestScore = e.Score;

            var message = e.Win
                ? $"Level clear! You scored {e.Score}."
                : $"Game over. Final score {e.Score}.";

            Application.Update(this, () =>
            {
                UpdateStatus(e.Score, 0, message);
                AlertBox.Show(message);
            });
        }

        private void UpdateStatus(int score, int lives, string extra)
        {
            var text = $"Score: {score}   Lives: {lives}   Best: {_bestScore}";
            if (!string.IsNullOrEmpty(extra))
                text += "   " + extra;
            this.labelStatus.Text = text;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.marioGame.ResetGame();
        }

        private void Page1_Disposed(object sender, EventArgs e)
        {
            this.marioGame.ScoreChanged -= MarioGame_ScoreChanged;
            this.marioGame.GameOver -= MarioGame_GameOver;
        }
    }
}
