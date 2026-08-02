using System;
using Wisej.Web;

namespace Wisej.StaticsExample
{
    /// <summary>
    /// A tiny Super Mario-like platformer rendered entirely in the browser via
    /// an HTML5 canvas. The C# side owns durable state (score, game over) and
    /// receives events over the websocket; the frame loop and per-frame drawing
    /// stay in the browser so movement is not gated by round trips.
    /// </summary>
    public class MarioGame : Widget
    {
        public MarioGame()
        {
            this.BorderStyle = BorderStyle.None;

            this.WiredEvents = new[] { "score", "gameOver", "won" };

            // The whole game lives in this script: level data, physics, drawing,
            // input. Options.resetVersion is bumped from C# to trigger a reset.
            this.InitScript = @"
this.init = function (options) {
    var widget = this;
    var container = this.container;
    container.style.background = '#5c94fc';
    container.style.overflow = 'hidden';
    container.style.outline = 'none';
    container.tabIndex = 0;

    var canvas = document.createElement('canvas');
    canvas.style.display = 'block';
    canvas.style.width = '100%';
    canvas.style.height = '100%';
    container.appendChild(canvas);
    var ctx = canvas.getContext('2d');

    var W = 800, H = 450;
    function resize() {
        W = container.clientWidth || 800;
        H = container.clientHeight || 450;
        canvas.width = W;
        canvas.height = H;
    }
    resize();
    widget._resize = resize;
    window.addEventListener('resize', resize);

    // ---------- World ----------
    var TILE = 32;
    var GROUND_Y = 12 * TILE;   // 384
    var WORLD_W = 120 * TILE;   // 3840

    // Ground with pits cut out of it.
    var pits = [
        { x: 900,  w: 96  },
        { x: 1700, w: 128 },
        { x: 2600, w: 96  },
        { x: 3300, w: 96  }
    ];
    var platforms = [];
    var groundX = 0;
    for (var i = 0; i < pits.length; i++) {
        platforms.push({ x: groundX, y: GROUND_Y, w: pits[i].x - groundX, h: 200, kind: 'ground' });
        groundX = pits[i].x + pits[i].w;
    }
    platforms.push({ x: groundX, y: GROUND_Y, w: WORLD_W - groundX, h: 200, kind: 'ground' });

    // Floating brick platforms.
    var bricks = [
        { x: 200,  y: 300, w: 96,  h: 16 },
        { x: 400,  y: 240, w: 96,  h: 16 },
        { x: 600,  y: 300, w: 96,  h: 16 },
        { x: 850,  y: 280, w: 160, h: 16 },
        { x: 1100, y: 240, w: 96,  h: 16 },
        { x: 1300, y: 300, w: 96,  h: 16 },
        { x: 1650, y: 260, w: 220, h: 16 },
        { x: 1950, y: 220, w: 96,  h: 16 },
        { x: 2150, y: 300, w: 96,  h: 16 },
        { x: 2350, y: 260, w: 96,  h: 16 },
        { x: 2550, y: 280, w: 160, h: 16 },
        { x: 2820, y: 240, w: 96,  h: 16 },
        { x: 3020, y: 300, w: 96,  h: 16 },
        { x: 3260, y: 260, w: 200, h: 16 },
        { x: 3550, y: 220, w: 96,  h: 16 }
    ];
    for (var i = 0; i < bricks.length; i++) {
        bricks[i].kind = 'brick';
        platforms.push(bricks[i]);
    }

    // Coins: 3 above each brick + a scattering at ground level.
    var coinTemplates = [];
    for (var i = 0; i < bricks.length; i++) {
        var b = bricks[i];
        for (var c = 0; c < 3; c++) {
            coinTemplates.push({ x: b.x + 16 + c * 32, y: b.y - 24 });
        }
    }
    for (var x = 300; x < WORLD_W - 200; x += 220) {
        // Skip coins that would land in a pit.
        var inPit = false;
        for (var j = 0; j < pits.length; j++) {
            if (x > pits[j].x - 24 && x < pits[j].x + pits[j].w + 24) { inPit = true; break; }
        }
        if (!inPit) coinTemplates.push({ x: x, y: GROUND_Y - 44 });
    }
    var coins = [];
    function buildCoins() {
        coins = [];
        for (var i = 0; i < coinTemplates.length; i++) {
            coins.push({ x: coinTemplates[i].x, y: coinTemplates[i].y, r: 8, taken: false });
        }
    }
    buildCoins();

    // Enemies. Each patrols within a range on the ground.
    var enemyTemplates = [
        { x: 500,  vx: -0.6, range: [420,  680]  },
        { x: 1200, vx:  0.6, range: [1100, 1400] },
        { x: 1450, vx: -0.6, range: [1420, 1620] },
        { x: 2050, vx:  0.6, range: [1900, 2200] },
        { x: 2380, vx: -0.6, range: [2280, 2500] },
        { x: 2900, vx:  0.6, range: [2780, 3050] },
        { x: 3480, vx: -0.6, range: [3400, 3600] }
    ];
    var enemies = [];
    function buildEnemies() {
        enemies = [];
        for (var i = 0; i < enemyTemplates.length; i++) {
            var t = enemyTemplates[i];
            enemies.push({
                x: t.x, y: GROUND_Y - 24, w: 24, h: 24,
                vx: t.vx, range: t.range, alive: true
            });
        }
    }
    buildEnemies();

    var flag = { x: WORLD_W - 96, y: GROUND_Y - 160, w: 6, h: 160 };

    // ---------- Player ----------
    var player = {
        x: 60, y: GROUND_Y - 40, w: 20, h: 28,
        vx: 0, vy: 0, onGround: false, facing: 1, walkT: 0
    };

    var keys = {};
    var camera = { x: 0 };
    var score = 0;
    var lives = 3;
    var state = 'playing';   // playing | dying | gameover | won
    var stateTimer = 0;

    // ---------- Input ----------
    function isTypingElsewhere() {
        var a = document.activeElement;
        if (!a) return false;
        var t = a.tagName;
        return (t === 'INPUT' || t === 'TEXTAREA' || a.isContentEditable) && a !== container;
    }
    function keyDown(e) {
        if (isTypingElsewhere()) return;
        keys[e.code] = true;
        if (['ArrowLeft','ArrowRight','ArrowUp','ArrowDown','Space','KeyA','KeyD','KeyW','KeyS'].indexOf(e.code) !== -1) {
            e.preventDefault();
        }
    }
    function keyUp(e) {
        if (isTypingElsewhere()) return;
        keys[e.code] = false;
    }
    window.addEventListener('keydown', keyDown);
    window.addEventListener('keyup', keyUp);
    widget._keyDown = keyDown;
    widget._keyUp = keyUp;
    container.addEventListener('mousedown', function () { container.focus(); });

    // ---------- Helpers ----------
    function aabb(a, b) {
        return a.x < b.x + b.w && a.x + a.w > b.x &&
               a.y < b.y + b.h && a.y + a.h > b.y;
    }
    function respawn() {
        player.x = 60; player.y = GROUND_Y - 40;
        player.vx = 0; player.vy = 0; player.facing = 1; player.walkT = 0;
        camera.x = 0;
        state = 'playing';
    }
    function die() {
        if (state !== 'playing') return;
        state = 'dying';
        stateTimer = 900;
        player.vy = -8;
        player.vx = 0;
    }

    widget._resetGame = function () {
        lives = 3;
        score = 0;
        buildCoins();
        buildEnemies();
        respawn();
        widget.fireWidgetEvent('score', { score: score });
    };

    // ---------- Update ----------
    function step(dt) {
        if (state === 'gameover' || state === 'won') return;

        if (state === 'dying') {
            stateTimer -= dt;
            player.vy += 0.4;
            player.y += player.vy;
            if (stateTimer <= 0) {
                lives--;
                if (lives <= 0) {
                    state = 'gameover';
                    widget.fireWidgetEvent('gameOver', { score: score });
                } else {
                    respawn();
                }
            }
            return;
        }

        // Input
        var speed = 3.4;
        var accel = 0.5;
        var wantLeft  = keys['ArrowLeft']  || keys['KeyA'];
        var wantRight = keys['ArrowRight'] || keys['KeyD'];
        var wantJump  = keys['Space'] || keys['ArrowUp'] || keys['KeyW'];

        if (wantLeft && !wantRight) {
            player.vx = Math.max(player.vx - accel, -speed); player.facing = -1;
        } else if (wantRight && !wantLeft) {
            player.vx = Math.min(player.vx + accel,  speed); player.facing = 1;
        } else {
            player.vx *= 0.8;
            if (Math.abs(player.vx) < 0.05) player.vx = 0;
        }

        if (wantJump && player.onGround) {
            player.vy = -9.2;
            player.onGround = false;
        }
        // Variable-height jump: cut short if the button is released while rising.
        if (!wantJump && player.vy < -3) player.vy = -3;

        // Gravity.
        player.vy += 0.5;
        if (player.vy > 12) player.vy = 12;

        // Horizontal collision.
        player.x += player.vx;
        for (var i = 0; i < platforms.length; i++) {
            var p = platforms[i];
            if (aabb(player, p)) {
                if (player.vx > 0) player.x = p.x - player.w;
                else if (player.vx < 0) player.x = p.x + p.w;
                player.vx = 0;
            }
        }

        // Vertical collision.
        player.y += player.vy;
        player.onGround = false;
        for (var i = 0; i < platforms.length; i++) {
            var p = platforms[i];
            if (aabb(player, p)) {
                if (player.vy > 0) {
                    player.y = p.y - player.h; player.vy = 0; player.onGround = true;
                } else if (player.vy < 0) {
                    player.y = p.y + p.h; player.vy = 0;
                }
            }
        }
        if (Math.abs(player.vx) > 0.1 && player.onGround) {
            player.walkT += Math.abs(player.vx) * 0.15;
        }

        // World bounds.
        if (player.x < 0) player.x = 0;
        if (player.x + player.w > WORLD_W) player.x = WORLD_W - player.w;

        // Fell in a pit.
        if (player.y > H + 300) die();

        // Coins.
        for (var i = 0; i < coins.length; i++) {
            var c = coins[i];
            if (c.taken) continue;
            if (player.x < c.x + c.r && player.x + player.w > c.x - c.r &&
                player.y < c.y + c.r && player.y + player.h > c.y - c.r) {
                c.taken = true;
                score += 10;
                widget.fireWidgetEvent('score', { score: score });
            }
        }

        // Enemies.
        for (var i = 0; i < enemies.length; i++) {
            var e = enemies[i];
            if (!e.alive) continue;
            e.x += e.vx;
            if (e.x < e.range[0]) { e.x = e.range[0]; e.vx =  Math.abs(e.vx); }
            if (e.x + e.w > e.range[1]) { e.x = e.range[1] - e.w; e.vx = -Math.abs(e.vx); }
            if (aabb(player, e)) {
                if (player.vy > 0 && (player.y + player.h - e.y) < 16) {
                    // Stomp.
                    e.alive = false;
                    player.vy = -7;
                    score += 100;
                    widget.fireWidgetEvent('score', { score: score });
                } else {
                    die();
                }
            }
        }

        // Flag.
        if (player.x + player.w > flag.x && player.y + player.h > flag.y) {
            state = 'won';
            score += 1000;
            widget.fireWidgetEvent('won', { score: score });
        }

        // Camera follows the player, clamped.
        camera.x = player.x - W * 0.4;
        if (camera.x < 0) camera.x = 0;
        if (camera.x > WORLD_W - W) camera.x = WORLD_W - W;
    }

    // ---------- Draw ----------
    function drawCloud(cx, cy) {
        ctx.fillStyle = '#fff';
        ctx.beginPath();
        ctx.arc(cx,      cy,     14, 0, Math.PI * 2);
        ctx.arc(cx + 16, cy - 6, 12, 0, Math.PI * 2);
        ctx.arc(cx + 30, cy,     14, 0, Math.PI * 2);
        ctx.arc(cx + 16, cy + 6, 14, 0, Math.PI * 2);
        ctx.fill();
    }

    function drawPlayer(sx, sy) {
        // Hat.
        ctx.fillStyle = '#e63946';
        ctx.fillRect(sx, sy, player.w, 6);
        ctx.fillRect(sx + (player.facing > 0 ? player.w - 4 : -6), sy + 2, 10, 5);
        // Face.
        ctx.fillStyle = '#ffcf9f';
        ctx.fillRect(sx + 2, sy + 6, player.w - 4, 8);
        // Eye.
        ctx.fillStyle = '#000';
        ctx.fillRect(sx + (player.facing > 0 ? player.w - 7 : 4), sy + 9, 2, 3);
        // Shirt.
        ctx.fillStyle = '#e63946';
        ctx.fillRect(sx, sy + 14, player.w, 6);
        // Overalls.
        ctx.fillStyle = '#1d4ed8';
        ctx.fillRect(sx + 2, sy + 18, player.w - 4, 8);
        // Legs (animate while walking).
        var swing = Math.sin(player.walkT) * 2;
        ctx.fillStyle = '#1d4ed8';
        ctx.fillRect(sx + 2,               sy + player.h - 4, 6, 4 + swing);
        ctx.fillRect(sx + player.w - 8,    sy + player.h - 4, 6, 4 - swing);
        // Shoes.
        ctx.fillStyle = '#5b3a1a';
        ctx.fillRect(sx + 1,               sy + player.h - 1, 8, 2);
        ctx.fillRect(sx + player.w - 9,    sy + player.h - 1, 8, 2);
    }

    function draw(t) {
        // Sky.
        var grad = ctx.createLinearGradient(0, 0, 0, H);
        grad.addColorStop(0, '#5c94fc');
        grad.addColorStop(1, '#a7d8ff');
        ctx.fillStyle = grad;
        ctx.fillRect(0, 0, W, H);

        // Clouds (parallax).
        var cloudOff = camera.x * 0.3;
        for (var i = 0; i < 14; i++) {
            var cx = ((i * 320) - cloudOff) % (W + 400);
            if (cx < -100) cx += W + 400;
            drawCloud(cx, 50 + (i % 3) * 22);
        }

        // Hills (parallax).
        ctx.fillStyle = '#4bb04b';
        var hillOff = camera.x * 0.5;
        for (var i = 0; i < 24; i++) {
            var hx = (i * 260) - (hillOff % 260);
            ctx.beginPath();
            ctx.arc(hx, GROUND_Y, 90, Math.PI, 0);
            ctx.fill();
        }

        // Platforms.
        for (var i = 0; i < platforms.length; i++) {
            var p = platforms[i];
            var sx = p.x - camera.x;
            if (sx + p.w < 0 || sx > W) continue;
            if (p.kind === 'ground') {
                ctx.fillStyle = '#8b4513';
                ctx.fillRect(sx, p.y, p.w, p.h);
                ctx.fillStyle = '#2ea52e';
                ctx.fillRect(sx, p.y, p.w, 8);
            } else {
                ctx.fillStyle = '#c67c3a';
                ctx.fillRect(sx, p.y, p.w, p.h);
                ctx.strokeStyle = '#5c3410';
                ctx.lineWidth = 1;
                for (var bx = 0; bx < p.w; bx += 16) {
                    ctx.strokeRect(sx + bx + 0.5, p.y + 0.5, 16, p.h - 1);
                }
            }
        }

        // Flag pole and banner.
        (function () {
            var sx = flag.x - camera.x;
            if (sx > -60 && sx < W + 60) {
                ctx.fillStyle = '#555';
                ctx.fillRect(sx, flag.y, flag.w, flag.h);
                ctx.fillStyle = '#e63946';
                ctx.beginPath();
                ctx.moveTo(sx + flag.w, flag.y + 4);
                ctx.lineTo(sx + flag.w + 44, flag.y + 20);
                ctx.lineTo(sx + flag.w, flag.y + 36);
                ctx.fill();
                // Ball on top.
                ctx.fillStyle = '#f2c94c';
                ctx.beginPath();
                ctx.arc(sx + flag.w / 2, flag.y - 2, 5, 0, Math.PI * 2);
                ctx.fill();
            }
        })();

        // Coins.
        for (var i = 0; i < coins.length; i++) {
            var c = coins[i];
            if (c.taken) continue;
            var sx = c.x - camera.x;
            if (sx < -20 || sx > W + 20) continue;
            var bob = Math.sin((t + i * 100) * 0.005) * 2;
            ctx.fillStyle = '#ffd800';
            ctx.beginPath();
            ctx.arc(sx, c.y + bob, c.r, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = '#b58900';
            ctx.fillRect(sx - 2, c.y + bob - 5, 4, 10);
        }

        // Enemies.
        for (var i = 0; i < enemies.length; i++) {
            var e = enemies[i];
            if (!e.alive) continue;
            var sx = e.x - camera.x;
            if (sx < -40 || sx > W + 40) continue;
            // Body.
            ctx.fillStyle = '#7a4b1a';
            ctx.fillRect(sx, e.y + 4, e.w, e.h - 4);
            // Feet.
            ctx.fillStyle = '#3a2410';
            ctx.fillRect(sx + 2,          e.y + e.h - 6, 6, 4);
            ctx.fillRect(sx + e.w - 8,    e.y + e.h - 6, 6, 4);
            // Eyes.
            ctx.fillStyle = '#fff';
            ctx.fillRect(sx + 5,        e.y + 8, 4, 5);
            ctx.fillRect(sx + e.w - 9,  e.y + 8, 4, 5);
            ctx.fillStyle = '#000';
            ctx.fillRect(sx + 6,        e.y + 10, 2, 3);
            ctx.fillRect(sx + e.w - 8,  e.y + 10, 2, 3);
        }

        // Player.
        drawPlayer(player.x - camera.x, player.y);

        // HUD bar.
        ctx.fillStyle = 'rgba(0,0,0,0.5)';
        ctx.fillRect(0, 0, W, 30);
        ctx.fillStyle = '#fff';
        ctx.font = 'bold 15px monospace';
        ctx.textBaseline = 'middle';
        ctx.textAlign = 'left';
        ctx.fillText('SCORE ' + score, 12, 15);
        ctx.fillText('LIVES ' + lives, 150, 15);
        ctx.textAlign = 'center';
        ctx.fillText('WORLD 1-1', W / 2, 15);
        ctx.textAlign = 'right';
        ctx.fillText('← → move   space jump', W - 12, 15);
        ctx.textAlign = 'left';

        if (state === 'gameover' || state === 'won') {
            ctx.fillStyle = 'rgba(0,0,0,0.6)';
            ctx.fillRect(0, 0, W, H);
            ctx.fillStyle = '#fff';
            ctx.textAlign = 'center';
            ctx.font = 'bold 40px monospace';
            ctx.fillText(state === 'won' ? 'YOU WIN!' : 'GAME OVER', W / 2, H / 2 - 20);
            ctx.font = 'bold 18px monospace';
            ctx.fillText('Final score ' + score, W / 2, H / 2 + 20);
            ctx.fillText('Press RESET to play again', W / 2, H / 2 + 50);
            ctx.textAlign = 'left';
        }
    }

    // ---------- Loop ----------
    var last = 0;
    function loop(t) {
        var dt = last ? Math.min(32, t - last) : 16;
        last = t;
        step(dt);
        draw(t);
        widget._raf = requestAnimationFrame(loop);
    }
    widget._raf = requestAnimationFrame(loop);

    setTimeout(function () { try { container.focus(); } catch (ex) {} }, 50);
};

this.update = function (options, old) {
    var v = options && options.resetVersion;
    var o = old && old.resetVersion;
    if (v !== o && this._resetGame) this._resetGame();
};

this.destroy = function () {
    if (this._raf) cancelAnimationFrame(this._raf);
    if (this._keyDown) window.removeEventListener('keydown', this._keyDown);
    if (this._keyUp)   window.removeEventListener('keyup',   this._keyUp);
    if (this._resize)  window.removeEventListener('resize',  this._resize);
};
";
        }

        private int _resetVersion;

        /// <summary>Raised each time the score changes.</summary>
        public event EventHandler<int> ScoreChanged;

        /// <summary>Raised when the player runs out of lives.</summary>
        public event EventHandler<int> GameOver;

        /// <summary>Raised when the player reaches the flag.</summary>
        public event EventHandler<int> Won;

        /// <summary>Restart the game from the beginning.</summary>
        public void ResetGame()
        {
            _resetVersion++;
            this.Options.resetVersion = _resetVersion;
            this.Update();
        }

        protected override void OnWidgetEvent(WidgetEventArgs e)
        {
            switch (e.Type)
            {
                case "score":
                    ScoreChanged?.Invoke(this, (int)e.Data.score);
                    break;
                case "gameOver":
                    GameOver?.Invoke(this, (int)e.Data.score);
                    break;
                case "won":
                    Won?.Invoke(this, (int)e.Data.score);
                    break;
                default:
                    base.OnWidgetEvent(e);
                    break;
            }
        }
    }
}
