using System;
using Wisej.Web;

namespace Wisej.StaticsExample
{
    /// <summary>
    /// A side-scrolling platformer rendered entirely in the browser on an HTML5 canvas.
    /// A Wisej.Web.Widget hosts the canvas and JavaScript loop; the server only receives
    /// coarse events (score changes, level cleared, game over) so state that matters
    /// lives in C# without one websocket message per frame per sprite.
    /// </summary>
    public class MarioGame : Widget
    {
        private int _resetToken;

        public MarioGame()
        {
            this.BorderStyle = BorderStyle.None;
            this.Size = new System.Drawing.Size(800, 400);

            this.WiredEvents = new[] { "scoreChanged", "gameOver" };

            this.Options.resetToken = 0;

            this.InitScript = @"
this.init = function(options) {
    var widget = this;
    widget._destroyed = false;

    var start = function() {
        var host = widget.container;
        if (!host || widget._destroyed) return;

        host.innerHTML = '';
        host.style.overflow = 'hidden';
        host.style.background = '#5c94fc';
        host.style.outline = 'none';
        host.setAttribute('tabindex', '0');

        var canvas = document.createElement('canvas');
        var VIEW_W = host.clientWidth || 800;
        var VIEW_H = host.clientHeight || 400;
        canvas.width = VIEW_W;
        canvas.height = VIEW_H;
        canvas.style.display = 'block';
        canvas.style.width = '100%';
        canvas.style.height = '100%';
        host.appendChild(canvas);
        var ctx = canvas.getContext('2d');

        var GROUND_Y = VIEW_H - 40;
        var LEVEL_W = 3200;

        var state;

        function resetLevel(keepStats) {
            var prevScore = state ? state.score : 0;
            var prevLives = state ? state.lives : 3;
            state = {
                player: { x: 40, y: GROUND_Y - 40, vx: 0, vy: 0, w: 22, h: 32, onGround: false, facing: 1 },
                cameraX: 0,
                score: keepStats ? prevScore : 0,
                lives: keepStats ? prevLives : 3,
                won: false,
                dead: false,
                deadTimer: 0,
                platforms: [
                    { x: 0,    y: GROUND_Y, w: LEVEL_W, h: 40 },
                    { x: 260,  y: GROUND_Y - 100, w: 100, h: 20 },
                    { x: 500,  y: GROUND_Y - 160, w: 100, h: 20 },
                    { x: 720,  y: GROUND_Y - 100, w: 80,  h: 20 },
                    { x: 1000, y: GROUND_Y - 140, w: 120, h: 20 },
                    { x: 1300, y: GROUND_Y - 80,  w: 90,  h: 20 },
                    { x: 1500, y: GROUND_Y - 200, w: 120, h: 20 },
                    { x: 1800, y: GROUND_Y - 120, w: 80,  h: 20 },
                    { x: 2050, y: GROUND_Y - 180, w: 100, h: 20 },
                    { x: 2300, y: GROUND_Y - 120, w: 80,  h: 20 },
                    { x: 2500, y: GROUND_Y - 200, w: 200, h: 20 },
                    { x: 2800, y: GROUND_Y - 60,  w: 60,  h: 20 }
                ],
                coins: [
                    { x: 300, y: GROUND_Y - 130 }, { x: 540, y: GROUND_Y - 190 },
                    { x: 750, y: GROUND_Y - 130 }, { x: 1060, y: GROUND_Y - 170 },
                    { x: 1350, y: GROUND_Y - 110 }, { x: 1560, y: GROUND_Y - 230 },
                    { x: 1840, y: GROUND_Y - 150 }, { x: 2090, y: GROUND_Y - 210 },
                    { x: 2340, y: GROUND_Y - 150 }, { x: 2560, y: GROUND_Y - 230 },
                    { x: 2620, y: GROUND_Y - 230 }, { x: 2680, y: GROUND_Y - 230 }
                ].map(function(c) { return { x: c.x, y: c.y, r: 8, taken: false, spin: Math.random() * Math.PI * 2 }; }),
                enemies: [
                    { x: 400,  y: GROUND_Y - 24, vx: -1,   w: 24, h: 24, alive: true, minX: 380,  maxX: 620 },
                    { x: 900,  y: GROUND_Y - 24, vx: -1.2, w: 24, h: 24, alive: true, minX: 850,  maxX: 1150 },
                    { x: 1400, y: GROUND_Y - 24, vx: 1,    w: 24, h: 24, alive: true, minX: 1250, maxX: 1550 },
                    { x: 1900, y: GROUND_Y - 24, vx: -1.5, w: 24, h: 24, alive: true, minX: 1700, maxX: 2000 },
                    { x: 2400, y: GROUND_Y - 24, vx: 1,    w: 24, h: 24, alive: true, minX: 2200, maxX: 2450 }
                ],
                flag: { x: LEVEL_W - 80, y: GROUND_Y - 200, w: 4, h: 200 }
            };
        }
        resetLevel(false);

        var keys = {};
        var keyName = function(e) { return (e.key || '').toLowerCase(); };
        var onKeyDown = function(e) {
            var k = keyName(e);
            keys[k] = true;
            if (k === 'arrowleft' || k === 'arrowright' || k === 'arrowup' || k === 'arrowdown' || k === ' ' || k === 'spacebar') {
                e.preventDefault();
            }
        };
        var onKeyUp = function(e) { keys[keyName(e)] = false; };
        host.addEventListener('keydown', onKeyDown);
        host.addEventListener('keyup', onKeyUp);
        host.addEventListener('mousedown', function() { host.focus(); });
        try { host.focus(); } catch (ex) {}

        widget._resetGame = function() { resetLevel(false); fireScore(); };
        widget._stopGame = function() {
            widget._destroyed = true;
            host.removeEventListener('keydown', onKeyDown);
            host.removeEventListener('keyup', onKeyUp);
        };

        function rectOverlap(ax, ay, aw, ah, bx, by, bw, bh) {
            return ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
        }

        function fireScore() {
            widget.fireWidgetEvent('scoreChanged', { score: state.score, lives: state.lives });
        }

        function step() {
            var p = state.player;

            if (state.dead || state.won) {
                state.deadTimer++;
                if (state.deadTimer > 90) {
                    if (state.won) {
                        widget.fireWidgetEvent('gameOver', { score: state.score, win: true });
                        resetLevel(false);
                        fireScore();
                    } else if (state.lives > 0) {
                        resetLevel(true);
                    } else {
                        widget.fireWidgetEvent('gameOver', { score: state.score, win: false });
                        resetLevel(false);
                        fireScore();
                    }
                }
                return;
            }

            var moveL = keys['arrowleft'] || keys['a'];
            var moveR = keys['arrowright'] || keys['d'];
            var jump  = keys['arrowup'] || keys['w'] || keys[' '] || keys['spacebar'];

            if (moveL)      { p.vx = -3.5; p.facing = -1; }
            else if (moveR) { p.vx = 3.5;  p.facing = 1;  }
            else            { p.vx *= 0.7; if (Math.abs(p.vx) < 0.05) p.vx = 0; }

            if (jump && p.onGround) { p.vy = -11.5; p.onGround = false; }

            p.vy += 0.6;
            if (p.vy > 14) p.vy = 14;

            // Horizontal move + collision.
            p.x += p.vx;
            if (p.x < 0) p.x = 0;
            if (p.x + p.w > LEVEL_W) p.x = LEVEL_W - p.w;
            for (var i = 0; i < state.platforms.length; i++) {
                var pl = state.platforms[i];
                if (rectOverlap(p.x, p.y, p.w, p.h, pl.x, pl.y, pl.w, pl.h)) {
                    if (p.vx > 0) p.x = pl.x - p.w;
                    else if (p.vx < 0) p.x = pl.x + pl.w;
                    p.vx = 0;
                }
            }

            // Vertical move + collision.
            p.y += p.vy;
            p.onGround = false;
            for (var j = 0; j < state.platforms.length; j++) {
                var pl2 = state.platforms[j];
                if (rectOverlap(p.x, p.y, p.w, p.h, pl2.x, pl2.y, pl2.w, pl2.h)) {
                    if (p.vy > 0) { p.y = pl2.y - p.h; p.vy = 0; p.onGround = true; }
                    else if (p.vy < 0) { p.y = pl2.y + pl2.h; p.vy = 0; }
                }
            }

            if (p.y > VIEW_H + 200) {
                state.lives--;
                state.dead = true;
                fireScore();
                return;
            }

            for (var c = 0; c < state.coins.length; c++) {
                var coin = state.coins[c];
                if (coin.taken) continue;
                coin.spin += 0.15;
                if (rectOverlap(p.x, p.y, p.w, p.h, coin.x - coin.r, coin.y - coin.r, coin.r * 2, coin.r * 2)) {
                    coin.taken = true;
                    state.score += 100;
                    fireScore();
                }
            }

            for (var k = 0; k < state.enemies.length; k++) {
                var en = state.enemies[k];
                if (!en.alive) continue;
                en.x += en.vx;
                if (en.x < en.minX)          { en.x = en.minX; en.vx = -en.vx; }
                if (en.x + en.w > en.maxX)   { en.x = en.maxX - en.w; en.vx = -en.vx; }

                if (rectOverlap(p.x, p.y, p.w, p.h, en.x, en.y, en.w, en.h)) {
                    if (p.vy > 0 && (p.y + p.h) - en.y < 16) {
                        en.alive = false;
                        p.vy = -8;
                        state.score += 200;
                        fireScore();
                    } else {
                        state.lives--;
                        state.dead = true;
                        fireScore();
                    }
                }
            }

            var f = state.flag;
            if (p.x + p.w > f.x && p.x < f.x + f.w + 20 && p.y + p.h > f.y) {
                state.won = true;
                state.score += 1000;
                fireScore();
            }

            state.cameraX = p.x - VIEW_W / 2 + p.w / 2;
            if (state.cameraX < 0) state.cameraX = 0;
            if (state.cameraX > LEVEL_W - VIEW_W) state.cameraX = LEVEL_W - VIEW_W;
        }

        function drawCloud(x, y) {
            ctx.beginPath();
            ctx.arc(x, y, 14, 0, Math.PI * 2);
            ctx.arc(x + 14, y - 6, 12, 0, Math.PI * 2);
            ctx.arc(x + 28, y, 14, 0, Math.PI * 2);
            ctx.arc(x + 14, y + 6, 10, 0, Math.PI * 2);
            ctx.fill();
        }

        function pad(n, w) { var s = String(n); while (s.length < w) s = '0' + s; return s; }

        function draw() {
            var cam = state.cameraX;

            ctx.fillStyle = '#5c94fc';
            ctx.fillRect(0, 0, VIEW_W, VIEW_H);

            ctx.fillStyle = '#ffffff';
            for (var i = 0; i < 10; i++) {
                var cx = ((i * 380) - cam * 0.3);
                cx = ((cx % (LEVEL_W + 400)) + LEVEL_W + 400) % (LEVEL_W + 400) - 200;
                drawCloud(cx, 40 + (i % 3) * 30);
            }

            ctx.fillStyle = '#4ea832';
            for (var h = 0; h < 10; h++) {
                var hx = h * 500 - cam * 0.5;
                ctx.beginPath();
                ctx.arc(hx + 100, GROUND_Y, 80, Math.PI, Math.PI * 2);
                ctx.fill();
            }

            for (var p2 = 0; p2 < state.platforms.length; p2++) {
                var pl = state.platforms[p2];
                var sx = pl.x - cam;
                if (sx + pl.w < 0 || sx > VIEW_W) continue;
                if (pl.y >= GROUND_Y) {
                    ctx.fillStyle = '#c07030';
                    ctx.fillRect(sx, pl.y, pl.w, pl.h);
                    ctx.fillStyle = '#7a3e1a';
                    ctx.fillRect(sx, pl.y, pl.w, 6);
                } else {
                    ctx.fillStyle = '#d2691e';
                    ctx.fillRect(sx, pl.y, pl.w, pl.h);
                    ctx.strokeStyle = '#000';
                    ctx.lineWidth = 1;
                    ctx.strokeRect(sx + 0.5, pl.y + 0.5, pl.w - 1, pl.h - 1);
                }
            }

            for (var ci = 0; ci < state.coins.length; ci++) {
                var coin = state.coins[ci];
                if (coin.taken) continue;
                var csx = coin.x - cam;
                if (csx + coin.r < 0 || csx - coin.r > VIEW_W) continue;
                var sq = Math.abs(Math.cos(coin.spin));
                ctx.fillStyle = '#ffd700';
                ctx.beginPath();
                ctx.ellipse(csx, coin.y, coin.r * sq + 1, coin.r, 0, 0, Math.PI * 2);
                ctx.fill();
                ctx.strokeStyle = '#b8860b';
                ctx.lineWidth = 1;
                ctx.stroke();
            }

            for (var ei = 0; ei < state.enemies.length; ei++) {
                var en = state.enemies[ei];
                if (!en.alive) continue;
                var esx = en.x - cam;
                ctx.fillStyle = '#8b4513';
                ctx.fillRect(esx, en.y, en.w, en.h);
                ctx.fillStyle = '#3a1e0a';
                ctx.fillRect(esx, en.y + en.h - 4, en.w, 4);
                ctx.fillStyle = '#fff';
                ctx.fillRect(esx + 4, en.y + 6, 6, 6);
                ctx.fillRect(esx + 14, en.y + 6, 6, 6);
                ctx.fillStyle = '#000';
                ctx.fillRect(esx + 6, en.y + 8, 2, 2);
                ctx.fillRect(esx + 16, en.y + 8, 2, 2);
            }

            var f = state.flag;
            var fsx = f.x - cam;
            ctx.fillStyle = '#444';
            ctx.fillRect(fsx, f.y, f.w, f.h);
            ctx.fillStyle = '#e91e1e';
            ctx.beginPath();
            ctx.moveTo(fsx + f.w, f.y);
            ctx.lineTo(fsx + f.w + 30, f.y + 15);
            ctx.lineTo(fsx + f.w, f.y + 30);
            ctx.fill();

            var pl3 = state.player;
            var psx = pl3.x - cam;
            // Hat
            ctx.fillStyle = '#e91e1e';
            ctx.fillRect(psx - 2, pl3.y, pl3.w + 4, 8);
            ctx.fillRect(psx + 4, pl3.y - 3, pl3.w - 8, 4);
            // Face
            ctx.fillStyle = '#f5c592';
            ctx.fillRect(psx + 2, pl3.y + 8, pl3.w - 4, 10);
            // Moustache
            ctx.fillStyle = '#3a2410';
            ctx.fillRect(psx + 4, pl3.y + 14, pl3.w - 8, 2);
            // Eye
            ctx.fillStyle = '#000';
            ctx.fillRect(psx + (pl3.facing > 0 ? pl3.w - 8 : 4), pl3.y + 10, 2, 3);
            // Overalls
            ctx.fillStyle = '#2848c0';
            ctx.fillRect(psx, pl3.y + 18, pl3.w, pl3.h - 18);
            // Overall straps
            ctx.fillStyle = '#2848c0';
            ctx.fillRect(psx + 4, pl3.y + 16, 4, 4);
            ctx.fillRect(psx + pl3.w - 8, pl3.y + 16, 4, 4);
            // Shoes
            ctx.fillStyle = '#3a2410';
            ctx.fillRect(psx, pl3.y + pl3.h - 4, pl3.w, 4);

            // HUD
            ctx.fillStyle = 'rgba(0,0,0,0.35)';
            ctx.fillRect(0, 0, VIEW_W, 30);
            ctx.fillStyle = '#fff';
            ctx.font = 'bold 14px monospace';
            ctx.textBaseline = 'middle';
            ctx.fillText('SCORE ' + pad(state.score, 6), 10, 15);
            ctx.fillText('LIVES x' + state.lives, 200, 15);
            ctx.fillText('WORLD 1-1', 340, 15);
            ctx.fillText('A/D or arrows to move, W/Space to jump', 460, 15);

            if (state.dead) {
                ctx.fillStyle = 'rgba(0,0,0,0.55)';
                ctx.fillRect(0, 0, VIEW_W, VIEW_H);
                ctx.fillStyle = '#fff';
                ctx.font = 'bold 32px monospace';
                ctx.textAlign = 'center';
                ctx.fillText(state.lives > 0 ? 'OOPS!' : 'GAME OVER', VIEW_W / 2, VIEW_H / 2);
                ctx.textAlign = 'left';
            } else if (state.won) {
                ctx.fillStyle = 'rgba(0,0,0,0.55)';
                ctx.fillRect(0, 0, VIEW_W, VIEW_H);
                ctx.fillStyle = '#fff200';
                ctx.font = 'bold 32px monospace';
                ctx.textAlign = 'center';
                ctx.fillText('LEVEL CLEAR!', VIEW_W / 2, VIEW_H / 2);
                ctx.textAlign = 'left';
            }
        }

        var running = true;
        widget._running = function() { return running && !widget._destroyed; };
        function loop() {
            if (!widget._running()) return;
            step();
            draw();
            requestAnimationFrame(loop);
        }
        loop();
        fireScore();
    };

    // Widget might not be attached to the DOM yet when init runs; wait for 'appear'.
    if (this.container && this.container.isConnected) {
        start();
    } else {
        this.addListenerOnce('appear', start);
    }
};

this.update = function(options, old) {
    if (!options || !old) return;
    if (options.resetToken !== old.resetToken && this._resetGame) {
        this._resetGame();
    }
};
";
        }

        /// <summary>
        /// Restart the level from scratch. Server-side reset button routes here.
        /// </summary>
        public void ResetGame()
        {
            this.Options.resetToken = ++_resetToken;
            this.Update();
        }

        /// <summary>
        /// Raised when the score or lives change in the browser.
        /// </summary>
        public event EventHandler<MarioScoreEventArgs> ScoreChanged;

        /// <summary>
        /// Raised when the level ends — either by reaching the flag (<see cref="MarioGameOverEventArgs.Win"/>
        /// = true) or by losing the last life.
        /// </summary>
        public event EventHandler<MarioGameOverEventArgs> GameOver;

        protected override void OnWidgetEvent(WidgetEventArgs e)
        {
            switch (e.Type)
            {
                case "scoreChanged":
                    ScoreChanged?.Invoke(this, new MarioScoreEventArgs(
                        Convert.ToInt32(e.Data.score),
                        Convert.ToInt32(e.Data.lives)));
                    break;

                case "gameOver":
                    GameOver?.Invoke(this, new MarioGameOverEventArgs(
                        Convert.ToInt32(e.Data.score),
                        Convert.ToBoolean(e.Data.win)));
                    break;

                default:
                    base.OnWidgetEvent(e);
                    break;
            }
        }
    }

    public class MarioScoreEventArgs : EventArgs
    {
        public int Score { get; }
        public int Lives { get; }

        public MarioScoreEventArgs(int score, int lives)
        {
            Score = score;
            Lives = lives;
        }
    }

    public class MarioGameOverEventArgs : EventArgs
    {
        public int Score { get; }
        public bool Win { get; }

        public MarioGameOverEventArgs(int score, bool win)
        {
            Score = score;
            Win = win;
        }
    }
}
