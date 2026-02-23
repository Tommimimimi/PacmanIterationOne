using System;
using System.Drawing;
using System.Windows.Forms;

namespace PacmanIterationOne
{
    public class NameInputForm : Form
    {
        private Label lblPrompt;
        private TextBox txtName;
        private Button buttConfirm;

        //stores name typed by player
        public string PlayerName { get; private set; } = "player";

        public NameInputForm(int finalScore)
        {
            //form setup
            this.Text = "game over";
            this.Size = new Size(360, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            //enter condition
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Confirm(); };

            //game over/score label
            lblPrompt = new Label
            {
                Text = $"game over\nscore: {finalScore}\n\nenter your name:",
                Font = new Font("Consolas", 12, FontStyle.Regular),
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(340, 100),
                Location = new Point(10, 15)
            };

            //name input popup box
            txtName = new TextBox
            {
                Font = new Font("Consolas", 13),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 16,
                TextAlign = HorizontalAlignment.Center,
                Size = new Size(220, 32),
                Location = new Point(70, 115)
            };

            //confirm button
            buttConfirm = new Button
            {
                Text = "save",
                Font = new Font("Consolas", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 36),
                Location = new Point(70, 155),
                Cursor = Cursors.Hand
            };
            buttConfirm.FlatAppearance.BorderColor = Color.FromArgb(120, 120, 120);
            buttConfirm.Click += (s, e) => Confirm();

            //decorative border on form
            this.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(220, 60, 20, 60), 2);
                e.Graphics.DrawRectangle(p, 5, 5, this.ClientSize.Width - 12, this.ClientSize.Height - 12);
            };

            this.Controls.Add(lblPrompt);
            this.Controls.Add(txtName);
            this.Controls.Add(buttConfirm);

            txtName.Focus();
        }

        private void Confirm()
        {
            //remove invalid characters
            string typed = txtName.Text.Trim();
            //default to player if invalid
            PlayerName = typed.Length > 0 ? typed : "player";
            //confirms entry
            this.DialogResult = DialogResult.OK;
            //close out of the form
            this.Close();
        }
    }
}
