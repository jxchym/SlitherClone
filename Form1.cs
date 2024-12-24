using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;

namespace SliterIOSnake
{
    public partial class Form1 : Form
    {
        private Leaderboard leaderboard = new Leaderboard();
        List<Fruit> foods = new List<Fruit>();
        Snake _snake;
        bool isGameRunning = false;
        private Label scoreLabel;
        private bool isShiftPressed = false;
        private DateTime lastSpeedBoostTime = DateTime.Now;
        private int maxscore = 0;
        private float defaultSpeed;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            _snake = new Snake(Width / 2, Height / 2);
            defaultSpeed = _snake.Speed;
            for (int i = 0; i < 5; i++)
            {
                _snake.Body.Add(new Point(Width / 2, Height / 2 + i * 10));
            }

            var startButton = new Button
            {
                Text = "Spustit",
                Size = new Size(100, 50),
                Location = new Point(Width / 2 - 50, Height / 2 - 25)
            };
            startButton.Click += StartButton_Click;
            Controls.Add(startButton);

            scoreLabel = new Label
            {
                Text = "Skóre: 0",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            //Controls.Add(scoreLabel);

            var infoLabel = new Label
            {
                Text = "shift - zrychlení\nčervená: +1\nsvětle zelená: +2\nzelená: +3\nzlatá: +1 a speed boost\nfialová: -1\nčerná: game over",
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, 50)
            };
            Controls.Add(infoLabel);

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 100;
            flashTimer.Tick += FlashTimer_Tick;
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            isGameRunning = true;

            foreach (Control control in Controls.OfType<Control>().ToList())
            {
                if (control != scoreLabel)
                {
                    Controls.Remove(control);
                }
            }

            if (!Controls.Contains(scoreLabel))
            {
                Controls.Add(scoreLabel);
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                isShiftPressed = true;
                _snake.Speed = defaultSpeed * 2;
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                isShiftPressed = false;
                _snake.Speed = defaultSpeed;
            }
        }

        static Random rand = new Random();

        private void AddFood()
        {
            var x = rand.Next(10, ClientSize.Width - 10);
            var y = rand.Next(10, ClientSize.Height - 10);
            var fruitType = rand.Next(0, 6);
            switch (fruitType)
            {
                case 0:
                    foods.Add(new Fruit(x, y, 10, Brushes.Red, 1, false));
                    break;
                case 1: 
                    foods.Add(new Fruit(x, y, 13, Brushes.LightGreen, 2, false));
                    break;
                case 2:
                    foods.Add(new Fruit(x, y, 8, Brushes.Purple, 1, true));
                    break;
                case 3:
                    foods.Add(new Fruit(x, y, 8, Brushes.Black, int.MaxValue, true));
                    break;
                case 4:
                    foods.Add(new Fruit(x, y, 17, Brushes.Green, 3, false));
                    break;
                case 5:
                    foods.Add(new Fruit(x, y, 15, Brushes.Gold, 2, false));
                    break;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (!isGameRunning)
            {
                return;
            }

            var g = e.Graphics;

            if (isFlashing)
            {
                BackColor = Color.Yellow;
            }
            else
            {
                BackColor = SystemColors.Control;
            }

            _snake.Draw(g);

            foreach (var fruit in foods)
            {
                fruit.Draw(g);
            }

            if (isFlashing)
            {
                var countdownText = countdown.ToString();
                var font = new Font("Arial", 48, FontStyle.Bold);
                var textSize = g.MeasureString(countdownText, font);
                var textPosition = new PointF((ClientSize.Width - textSize.Width) / 2, (ClientSize.Height - textSize.Height) / 2);
                g.DrawString(countdownText, font, Brushes.Black, textPosition);
            }
        }

        int foodCount = 0;
        private bool isFlashing = false;
        private DateTime flashEndTime;
        private System.Windows.Forms.Timer flashTimer;
        private int countdown = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!isGameRunning)
            {
                return;
            }

            if (foods.Count < 10 && foodCount < 10)
            {
                if (rand.Next(0, 10) == 1)
                {
                    AddFood();
                    foodCount++;
                }
            }

            for (int i = foods.Count - 1; i >= 0; i--)
            {
                var fruit = foods[i];
                if ((DateTime.Now - fruit.CreatedAt).TotalSeconds > 10) 
                {
                    foods.RemoveAt(i); 
                    foodCount--; 
                }
            }

            if (isShiftPressed && (DateTime.Now - lastSpeedBoostTime).TotalMilliseconds > 350)
            {
                if (_snake.Body.Count > 0)
                {
                    _snake.Body.RemoveAt(_snake.Body.Count - 1); 
                }

                lastSpeedBoostTime = DateTime.Now; 
            }

            _snake.Move(PointToClient(Cursor.Position), ClientSize.Width, ClientSize.Height);
            SelfCollision(); 

            for (int i = foods.Count - 1; i >= 0; i--)
            {
                var fruit = foods[i];
                var dx = _snake.Head.X - fruit.Position.X;
                var dy = _snake.Head.Y - fruit.Position.Y;
                var distance = MathF.Sqrt(dx * dx + dy * dy);

                if (distance < (10 + fruit.Size / 2))
                {
                    if (fruit.IsPoisonous)
                    {
                        if (_snake.Body.Count > 0)
                        {
                            if (_snake.Body.Count < fruit.Value)
                            {
                                GameOver();
                            }
                            else
                            {
                                _snake.Body.RemoveAt(_snake.Body.Count - 1);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < fruit.Value; j++)
                        {
                            _snake.Body.Add(_snake.Body[^1]);
                        }

                        if (fruit.Color == Brushes.Gold)
                        {
                            isFlashing = true;
                            flashEndTime = DateTime.Now.AddSeconds(3);
                            _snake.Speed = defaultSpeed * 2;
                            countdown = 3;
                            flashTimer.Start();
                            BackColor = Color.LightYellow;
                        }
                    }

                    foods.RemoveAt(i);
                    foodCount--;
                }
            }

            if (_snake.Body.Count <= 0)
            {
                GameOver();
            }

            if (_snake.Body.Count > maxscore)
            {
                maxscore = _snake.Body.Count;
            }
            scoreLabel.Text = $"Skóre: {_snake.Body.Count}";

            Invalidate();
        }

        private void SelfCollision()
        {
            for (int i = 1; i < _snake.Body.Count; i++)
            {
                var segment = _snake.Body[i];
                var dx = _snake.Head.X - segment.X;
                var dy = _snake.Head.Y - segment.Y;
                var distance = MathF.Sqrt(dx * dx + dy * dy);

                if (distance < 5)
                {
                    for (int j = i; j < _snake.Body.Count; j++)
                    {
                        var bodyPart = _snake.Body[j];
                        foods.Add(new Fruit((int)bodyPart.X, (int)bodyPart.Y, 10, Brushes.Orange, 1, false));
                    }

                    _snake.Body.RemoveRange(i, _snake.Body.Count - i);
                    break; 
                }
            }
        }

        private void GameOver()
        {
            isGameRunning = false;
            isFlashing = false;
            flashTimer.Stop();
            BackColor = Color.White;
            Controls.Clear();

            leaderboard.AddScore(maxscore);

            var endScoreLabel = new Label
            {
                Text = $"Konečné nejvyšší skóre: {maxscore}",
                AutoSize = true,
                Location = new Point(Width / 2 - 50, Height / 2 - 25)
            };
            Controls.Add(endScoreLabel);

            var leaderboardLabel = new Label
            {
                Text = "Leaderboard:\n" + string.Join("\n", leaderboard.GetTopScores().Select((score, index) => $"{index + 1}. {score}")),
                AutoSize = true,
                Location = new Point(Width / 2 - 50, Height / 2)
            };
            Controls.Add(leaderboardLabel);

            var infoLabel = new Label
            {
                Text = "shift - zrychlení\nčervená: +1\nsvětle zelená: +2\nzelená: +3\nzlatá: +1 a speed boost\nfialová: -1\nčerná: game over",
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, 50)
            };
            Controls.Add(infoLabel);

            maxscore = 0;

            var startButton = new Button
            {
                Text = "Spustit",
                Size = new Size(100, 50),
                Location = new Point(Width / 2 - 50, Height / 2 + 100)
            };
            startButton.Click += (s, e) =>
            {
                Controls.Clear();
                ResetGame();
                isGameRunning = true;
            };
            Controls.Add(startButton);
        }

        private void ResetGame()
        {
            maxscore = 0;
            _snake = new Snake(Width / 2, Height / 2);
            _snake.Body.Clear();
            for (int i = 0; i < 5; i++)
            {
                _snake.Body.Add(new Point(Width / 2, Height / 2 + i * 10));
            }
            foods.Clear();
            foodCount = 0;
            scoreLabel.Text = "Skóre: 0";
            Controls.Add(scoreLabel);
        }
        private void FlashTimer_Tick(object? sender, EventArgs e)
        {
            if (DateTime.Now >= flashEndTime)
            {
                isFlashing = false;
                _snake.Speed = defaultSpeed;
                flashTimer.Stop();
                BackColor = SystemColors.Control; 
            }
            else
            {
                countdown = (int)(flashEndTime - DateTime.Now).TotalSeconds;
                Invalidate();
            }
        }

        public class Fruit
        {
            public Point Position { get; private set; }
            public int Size { get; }
            public Brush Color { get; }
            public int Value { get; }
            public bool IsPoisonous { get; }
            public DateTime CreatedAt { get; }

            public Fruit(int x, int y, int size, Brush color, int value, bool isPoisonous)
            {
                Position = new Point(x, y);
                Size = size;
                Color = color;
                Value = value;
                IsPoisonous = isPoisonous;
                CreatedAt = DateTime.Now;
            }

            public void Reposition(int x, int y)
            {
                Position = new Point(x, y);
            }

            public void Draw(Graphics g)
            {
                g.FillEllipse(Color, Position.X - Size / 2, Position.Y - Size / 2, Size, Size);
            }
        }
        public class Leaderboard
        {
            private List<int> scores = new List<int>();
            private const string FilePath = "leaderboard.json";

            public Leaderboard()
            {
                LoadScores();
            }

            public void AddScore(int score)
            {
                scores.Add(score);
                scores = scores.OrderByDescending(s => s).Take(5).ToList();
                SaveScores();
            }

            public List<int> GetTopScores()
            {
                return scores;
            }

            private void SaveScores()
            {
                var json = JsonSerializer.Serialize(scores);
                File.WriteAllText(FilePath, json);
            }

            private void LoadScores()
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    scores = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
                }
            }
        }
    }
}
