using System;
using Wisej.Web;

namespace Wisej.StaticsExample
{
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void Page1_Load(object sender, EventArgs e)
        {
            gameCanvas.ScoreChanged += GameCanvas_ScoreChanged;
            gameCanvas.GameOver     += GameCanvas_GameOver;
            gameCanvas.Won          += GameCanvas_Won;
        }

        private void GameCanvas_ScoreChanged(object sender, int score)
        {
            Application.Update(this, () =>
            {
                this.labelStatus.Text = $"Score: {score}";
            });
        }

        private void GameCanvas_GameOver(object sender, int score)
        {
            Application.Update(this, () =>
            {
                this.labelStatus.Text = $"Game over — final score: {score}";
            });
        }

        private void GameCanvas_Won(object sender, int score)
        {
            Application.Update(this, () =>
            {
                this.labelStatus.Text = $"You reached the flag! Score: {score}";
            });
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.labelStatus.Text = "Score: 0";
            gameCanvas.ResetGame();
        }

        private void Page1_Disposed(object sender, EventArgs e)
        {
            gameCanvas.ScoreChanged -= GameCanvas_ScoreChanged;
            gameCanvas.GameOver     -= GameCanvas_GameOver;
            gameCanvas.Won          -= GameCanvas_Won;
        }
    }
}
