using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PacmanIterationOne;

namespace PacmanIterationOne
{
    //displays the top 10 leaderboard scores after a game ends
    public class LeaderboardForm : Form
    {
        private Label lblTitle;
        private Panel pnlScores;
        private Button btnMenu;

        public LeaderboardForm()
        {
            //form setup
            this.Text = "leaderboard";
            this.Size = new Size(480, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;

            //title
            lblTitle = new Label
            {
                Text = "LEADERBOARD",
                Font = new Font("RETROTECH", 26, FontStyle.Regular),
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(460, 60),
                Location = new Point(10, 25)
            };

            //scrollable panel to hold score rows
            pnlScores = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(440, 400),
                BackColor = Color.Transparent
            };

            //back to menu button
            btnMenu = new Button
            {
                Text = "MAIN MENU",
                Font = new Font("Consolas", 13, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 30),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 46),
                Location = new Point(140, 510),
                Cursor = Cursors.Hand
            };
            btnMenu.FlatAppearance.BorderColor = Color.FromArgb(120, 120, 120);
            btnMenu.FlatAppearance.BorderSize = 2;
            btnMenu.Click += (s, e) =>
            {
                this.Close();
            };

            //decorative border
            this.Paint += PaintBorder;

            this.Controls.Add(lblTitle);
            this.Controls.Add(pnlScores);
            this.Controls.Add(btnMenu);

            PopulateScores();
        }

        private void PopulateScores()
        {
            pnlScores.Controls.Clear();

            List<LeaderBoardEntry> entries = FileManager.ReadScores();

            //column header row
            Label header = new Label
            {
                Text = $"{"#",-4}{"name",-18}{"score",-10}{"date",-18}",
                Font = new Font("Consolas", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 160, 160),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(440, 24),
                Location = new Point(0, 0)
            };
            pnlScores.Controls.Add(header);

            //divider line
            Label divider = new Label
            {
                Text = new string('─', 54),
                Font = new Font("Consolas", 10),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(440, 18),
                Location = new Point(0, 26)
            };
            pnlScores.Controls.Add(divider);

            if (entries.Count == 0)
            {
                //empty state
                Label empty = new Label
                {
                    Text = "no scores yet",
                    Font = new Font("Consolas", 11, FontStyle.Italic),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(140, 80)
                };
                pnlScores.Controls.Add(empty);
                return;
            }

            //one row per entry
            for (int i = 0; i < entries.Count; i++)
            {
                LeaderBoardEntry entry = entries[i];

                //highlight top 3 in different colours
                Color rowColour = i switch
                {
                    0 => Color.Gold,
                    1 => Color.Silver,
                    2 => Color.FromArgb(205, 127, 50),
                    _ => Color.White
                };

                string rank = $"{i + 1}.";
                string name = entry.Name.Length > 14 ? entry.Name[..14] : entry.Name;
                string score = entry.Score.ToString();
                string date = entry.Date;

                Label row = new Label
                {
                    Text = $"{rank,-4}{name,-18}{score,-10}{date,-18}",
                    Font = new Font("Consolas", 11, FontStyle.Regular),
                    ForeColor = rowColour,
                    BackColor = Color.Transparent,
                    AutoSize = false,
                    Size = new Size(440, 26),
                    Location = new Point(0, 48 + i * 30)
                };
                pnlScores.Controls.Add(row);
            }
        }

        //paints a decorative border matching the rest of the ui
        private void PaintBorder(object? sender, PaintEventArgs e)
        {
            using Pen borderPen = new Pen(Color.FromArgb(220, 60, 20, 60), 3);
            e.Graphics.DrawRectangle(borderPen, 8, 8, this.ClientSize.Width - 18, this.ClientSize.Height - 18);

            using Pen innerPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1);
            e.Graphics.DrawRectangle(innerPen, 13, 13, this.ClientSize.Width - 28, this.ClientSize.Height - 28);
        }
    }
}