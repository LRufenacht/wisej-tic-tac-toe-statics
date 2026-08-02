using System;
using System.Collections.Generic;
using System.Drawing;
using Wisej.Web;

namespace Wisej.StaticsExample
{
    public partial class Page1 : Page
    {
        // Physics tuned for a 40 ms game tick.
        private const int Gravity = 1;
        private const int JumpVelocity = -15;
        private const int MoveSpeed = 5;
        private const int PlayerWidth = 24;
        private const int PlayerHeight = 32;
        private const int EnemyWidth = 26;
        private const int EnemyHeight = 22;
        private const int EnemySpeed = 2;

        private Panel _player;
        private Panel _enemy;
        private int _enemyDir = 1;
        private int _enemyMinX;
        private int _enemyMaxX;

        private readonly List<Panel> _platforms = new List<Panel>();
        private readonly List<Panel> _coins = new List<Panel>();
        private readonly HashSet<Keys> _keysDown = new HashSet<Keys>();

        private int _velY;
        private bool _onGround;
        private int _score;
        private Point _playerStart;

        public Page1()
        {
            InitializeComponent();
        }

        private void Page1_Load(object sender, EventArgs e)
        {
            Application.Title = "Platformer Adventure";

            BuildLevel();

            this.gameArea.KeyDown += GameArea_KeyDown;
            this.gameArea.KeyUp += GameArea_KeyUp;
            this.gameArea.Focus();

            this.gameTimer.Start();
        }

        private void BuildLevel()
        {
            _playerStart = new Point(30, 340);

            // Ground.
            AddPlatform(0, 360, 760, 20, Color.ForestGreen);

            // Floating platforms.
            AddPlatform(120, 300, 100, 15, Color.SaddleBrown);
            AddPlatform(270, 240, 100, 15, Color.SaddleBrown);
            AddPlatform(420, 290, 100, 15, Color.SaddleBrown);
            AddPlatform(560, 220, 100, 15, Color.SaddleBrown);
            AddPlatform(650, 140, 100, 15, Color.SaddleBrown);

            // Coins hovering just above each platform.
            AddCoin(150, 270);
            AddCoin(300, 210);
            AddCoin(450, 260);
            AddCoin(590, 190);
            AddCoin(680, 110);

            // Patrolling enemy on the ground.
            _enemyMinX = 300;
            _enemyMaxX = 540;
            _enemy = new Panel
            {
                BackColor = Color.SaddleBrown,
                Location = new Point(_enemyMinX, 360 - EnemyHeight),
                Size = new Size(EnemyWidth, EnemyHeight),
            };
            this.gameArea.Controls.Add(_enemy);

            // Player added last so it stays on top.
            _player = new Panel
            {
                BackColor = Color.Firebrick,
                Location = _playerStart,
                Size = new Size(PlayerWidth, PlayerHeight),
            };
            this.gameArea.Controls.Add(_player);
        }

        private void AddPlatform(int x, int y, int w, int h, Color color)
        {
            var p = new Panel
            {
                BackColor = color,
                Location = new Point(x, y),
                Size = new Size(w, h),
            };
            this.gameArea.Controls.Add(p);
            _platforms.Add(p);
        }

        private void AddCoin(int x, int y)
        {
            var c = new Panel
            {
                BackColor = Color.Gold,
                Location = new Point(x, y),
                Size = new Size(14, 14),
            };
            this.gameArea.Controls.Add(c);
            _coins.Add(c);
        }

        private void GameArea_KeyDown(object sender, KeyEventArgs e)
        {
            _keysDown.Add(e.KeyCode);
            if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Up) && _onGround)
            {
                _velY = JumpVelocity;
                _onGround = false;
            }
            e.Handled = true;
        }

        private void GameArea_KeyUp(object sender, KeyEventArgs e)
        {
            _keysDown.Remove(e.KeyCode);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            int velX = 0;
            if (_keysDown.Contains(Keys.Left))
                velX = -MoveSpeed;
            if (_keysDown.Contains(Keys.Right))
                velX = MoveSpeed;

            var loc = _player.Location;

            // Horizontal movement + collision.
            var newX = loc.X + velX;
            if (newX < 0) newX = 0;
            if (newX + PlayerWidth > this.gameArea.Width)
                newX = this.gameArea.Width - PlayerWidth;

            var hRect = new Rectangle(newX, loc.Y, PlayerWidth, PlayerHeight);
            foreach (var p in _platforms)
            {
                var pr = new Rectangle(p.Location, p.Size);
                if (!hRect.IntersectsWith(pr)) continue;
                if (velX > 0)
                    newX = pr.Left - PlayerWidth;
                else if (velX < 0)
                    newX = pr.Right;
            }

            // Vertical movement + collision.
            _velY += Gravity;
            var newY = loc.Y + _velY;
            _onGround = false;

            var vRect = new Rectangle(newX, newY, PlayerWidth, PlayerHeight);
            foreach (var p in _platforms)
            {
                var pr = new Rectangle(p.Location, p.Size);
                if (!vRect.IntersectsWith(pr)) continue;
                if (_velY > 0 && loc.Y + PlayerHeight <= pr.Top)
                {
                    newY = pr.Top - PlayerHeight;
                    _velY = 0;
                    _onGround = true;
                }
                else if (_velY < 0 && loc.Y >= pr.Bottom)
                {
                    newY = pr.Bottom;
                    _velY = 0;
                }
            }

            // Clamp to the game area.
            if (newY + PlayerHeight > this.gameArea.Height)
            {
                newY = this.gameArea.Height - PlayerHeight;
                _velY = 0;
                _onGround = true;
            }
            if (newY < 0)
            {
                newY = 0;
                _velY = 0;
            }

            _player.Location = new Point(newX, newY);

            // Move the goomba.
            var eLoc = _enemy.Location;
            var eNewX = eLoc.X + _enemyDir * EnemySpeed;
            if (eNewX < _enemyMinX)
            {
                eNewX = _enemyMinX;
                _enemyDir = 1;
            }
            else if (eNewX > _enemyMaxX)
            {
                eNewX = _enemyMaxX;
                _enemyDir = -1;
            }
            _enemy.Location = new Point(eNewX, eLoc.Y);

            var playerRect = new Rectangle(newX, newY, PlayerWidth, PlayerHeight);

            // Coin collection.
            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                var coin = _coins[i];
                if (playerRect.IntersectsWith(new Rectangle(coin.Location, coin.Size)))
                {
                    this.gameArea.Controls.Remove(coin);
                    coin.Dispose();
                    _coins.RemoveAt(i);
                    _score++;
                    this.scoreLabel.Text = "Coins: " + _score;
                }
            }

            // Enemy contact = respawn at the start.
            if (playerRect.IntersectsWith(new Rectangle(_enemy.Location, _enemy.Size)))
            {
                _player.Location = _playerStart;
                _velY = 0;
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.gameTimer.Stop();

            _keysDown.Clear();
            _velY = 0;
            _onGround = false;
            _score = 0;
            this.scoreLabel.Text = "Coins: 0";

            this.gameArea.Controls.Clear();
            _platforms.Clear();
            _coins.Clear();
            _player = null;
            _enemy = null;

            BuildLevel();
            this.gameArea.Focus();
            this.gameTimer.Start();
        }
    }
}
