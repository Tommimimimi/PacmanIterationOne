using pIterationOne;

namespace PacmanIterationOne
{
    public class MainMenuForm : Form
    {
        //main menu panels
        private Panel pnlMain;
        private Panel pnlOptions;

        //labels

        //main menu labels
        private Label lblTitle;
        private Label lblSubtitle;


        //options panel labels
        private Label lblOptionsTitle;
        private Label lblDifficulty;
        private Label lblMazeSize;
        private Label lblColour;
        private Label lblDebug;

        //buttons

        //main menu buttons
        private Button buttPlay;
        private Button buttOptions;
        private Button buttLeaderboard;
        private Button buttQuit;

        //difficulty buttons
        private Button buttEasy;
        private Button buttNormal;
        private Button buttHard;

        //maze size buttons
        private Button buttSmall;
        private Button buttMedium;
        private Button buttLarge;

        //colour buttons
        private Button buttPurple;
        private Button buttBlue;
        private Button buttGreen;
        private Button buttRed;
        private Button buttOrange;
        private Button buttRandom;

        //button to exit options
        private Button buttBack;

        //tracks current state of difficulty, maze size, and maze colour
        private Button selectedDifficulty;
        private Button selectedSize;
        private Button selectedColour;

        //misc objs used
        private CheckBox chkDebug;

        //preset colours to use for painting menu obj
        private readonly Color colBackground = Color.Black;
        private readonly Color colAccent = Color.FromArgb(220, 60, 20, 60);
        private readonly Color colText = Color.White;

        //these are for buttons that are selected or being hovered over
        private readonly Color colSelected = Color.FromArgb(255, 180, 0, 180);
        private readonly Color colHover = Color.FromArgb(80, 80, 80);

        //normal grey colours for buttons not selected or hovered over
        private readonly Color colButton = Color.FromArgb(30, 30, 30);
        private readonly Color colBorder = Color.FromArgb(120, 120, 120);

        public MainMenuForm()
        {
            InitMenu();
        }

        private void InitMenu()
        {
            //form setup
            this.Text = "Pac-Pac";  //renames form from form1 when hovered over in app bar
            this.Size = new Size(480, 580); //gives ratio which represents vert more than horiz
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = colBackground;
            this.FormBorderStyle = FormBorderStyle.None;

            //prevent flickering, very important for panels
            this.DoubleBuffered = true;

            //exit on escape key
            this.KeyPreview = true; //do not remove, can lead to keys not applying
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Application.Exit(); }; //hooks escape key to exiting the app

            BuildMainPanel();
            BuildOptionsPanel();

            //start on the main panel
            this.Controls.Add(pnlOptions);
            this.Controls.Add(pnlMain);
            pnlOptions.Visible = false;
        }

        private void BuildMainPanel()
        {
            pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colBackground
            };

            //title label
            lblTitle = new Label
            {
                Text = "PAC-PAC",
                Font = new Font("RETROTECH", 42, FontStyle.Regular),
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(460, 80),
                Location = new Point(10, 60)
            };

            //subtitle label
            lblSubtitle = new Label
            {
                Text = "a rogue-like maze chase game",
                Font = new Font("Consolas", 12, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(460, 28),
                Location = new Point(10, 145)
            };

            //play button
            buttPlay = CreateMenuButton("PLAY", new Point(140, 200));
            buttPlay.Click += (s, e) => StartGame();

            //options button
            buttOptions = CreateMenuButton("OPTIONS", new Point(140, 270));
            buttOptions.Click += (s, e) => ShowOptions();

            //leaderboard button
            buttLeaderboard = CreateMenuButton("LEADERBOARD", new Point(140, 340));
            buttLeaderboard.Font = new Font("Consolas", 13, FontStyle.Bold);
            buttLeaderboard.Click += (s, e) =>
            {
                //show leaderboard without closing the menu so the player can return
                LeaderboardForm lb = new LeaderboardForm();
                lb.FormClosed += (ls, le) => this.Show();
                this.Hide();
                lb.Show();
            };

            //quit button
            buttQuit = CreateMenuButton("QUIT", new Point(140, 410));
            buttQuit.Click += (s, e) => Application.Exit();

            //decorative border painted around the form
            pnlMain.Paint += PaintMenuBorder;

            pnlMain.Controls.Add(lblTitle);
            pnlMain.Controls.Add(lblSubtitle);
            pnlMain.Controls.Add(buttPlay);
            pnlMain.Controls.Add(buttOptions);
            pnlMain.Controls.Add(buttLeaderboard);
            pnlMain.Controls.Add(buttQuit);
        }

        //function to create options panel, coded in order of appearance (top to bottom):
        //panel setup -> title -> difficulty -> maze size -> wall colours -> debug toggle
        private void BuildOptionsPanel()
        {
            //options panel initializer
            pnlOptions = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colBackground
            };

            //options title initializer
            lblOptionsTitle = new Label
            {
                Text = "OPTIONS",
                Font = new Font("RETROTECH", 28, FontStyle.Regular),
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(460, 60),
                Location = new Point(10, 25)
            };

            //difficulty section label
            lblDifficulty = CreateSectionLabel("DIFFICULTY", new Point(40, 100));

            //difficulty button setup:

                buttEasy = CreateOptionButton("easy", new Point(40, 132));
                buttNormal = CreateOptionButton("normal", new Point(170, 132));
                buttHard = CreateOptionButton("hard", new Point(300, 132));

                //hook difficulty changes to respected buttons(using predetermined settings)
                buttEasy.Click += (s, e) => SelectDifficulty(buttEasy, GameSettings.DifficultyLevel.Easy);
                buttNormal.Click += (s, e) => SelectDifficulty(buttNormal, GameSettings.DifficultyLevel.Normal);
                buttHard.Click += (s, e) => SelectDifficulty(buttHard, GameSettings.DifficultyLevel.Hard);

            //maze size section label
            lblMazeSize = CreateSectionLabel("MAZE SIZE", new Point(40, 190));

            //size button setup:

                buttSmall = CreateOptionButton("small", new Point(40, 222));
                buttMedium = CreateOptionButton("medium", new Point(170, 222));
                buttLarge = CreateOptionButton("large", new Point(300, 222));

                //hook size changes to respected buttons(using predetermined settings)
                buttSmall.Click += (s, e) => SelectSize(buttSmall, GameSettings.MazeSize.Small);
                buttMedium.Click += (s, e) => SelectSize(buttMedium, GameSettings.MazeSize.Medium);
                buttLarge.Click += (s, e) => SelectSize(buttLarge, GameSettings.MazeSize.Large);
            
            //wall colour section label
            lblColour = CreateSectionLabel("WALL COLOUR", new Point(40, 280));

            //colour preset button setup:

                //first row
                buttPurple = CreateColourButton("purple", new Point(40, 312), Color.FromArgb(80, 20, 100));
                buttBlue = CreateColourButton("blue", new Point(150, 312), Color.FromArgb(20, 40, 120));
                buttGreen = CreateColourButton("green", new Point(260, 312), Color.FromArgb(20, 100, 30));

                //second row
                buttRed = CreateColourButton("red", new Point(40, 358), Color.FromArgb(120, 20, 20));
                buttOrange = CreateColourButton("orange", new Point(150, 358), Color.FromArgb(130, 65, 10));
                buttRandom = CreateColourButton("random", new Point(260, 358), Color.FromArgb(40, 40, 40));

                //hook colour changes to respected buttons(using colour presets)
                buttPurple.Click += (s, e) => SelectColour(buttPurple, GameSettings.ColourPreset.Purple);
                buttBlue.Click += (s, e) => SelectColour(buttBlue, GameSettings.ColourPreset.Blue);
                buttGreen.Click += (s, e) => SelectColour(buttGreen, GameSettings.ColourPreset.Green);
                buttRed.Click += (s, e) => SelectColour(buttRed, GameSettings.ColourPreset.Red);
                buttOrange.Click += (s, e) => SelectColour(buttOrange, GameSettings.ColourPreset.Orange);
                buttRandom.Click += (s, e) => SelectColour(buttRandom, GameSettings.ColourPreset.Random);

            //debug panel title for checkbox
            lblDebug = CreateSectionLabel("DEBUG PANEL", new Point(40, 410));

            //debug checkbox initializer
            chkDebug = new CheckBox
            {
                Text = "show debug interface",
                Font = new Font("Consolas", 11),
                ForeColor = colText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(40, 435),
                Checked = GameSettings.ShowDebugPanel
            };
            chkDebug.CheckedChanged += (s, e) => GameSettings.ShowDebugPanel = chkDebug.Checked;

            //back button
            buttBack = CreateMenuButton("BACK", new Point(140, 490));
            buttBack.Font = new Font("Consolas", 14, FontStyle.Bold);
            buttBack.Size = new Size(200, 48);
            buttBack.Click += (s, e) => ShowMain();

            pnlOptions.Paint += PaintMenuBorder;

            //add all resources into form
            pnlOptions.Controls.Add(lblOptionsTitle);
            pnlOptions.Controls.Add(lblDifficulty);
            pnlOptions.Controls.Add(buttEasy);
            pnlOptions.Controls.Add(buttNormal);
            pnlOptions.Controls.Add(buttHard);
            pnlOptions.Controls.Add(lblMazeSize);
            pnlOptions.Controls.Add(buttSmall);
            pnlOptions.Controls.Add(buttMedium);
            pnlOptions.Controls.Add(buttLarge);
            pnlOptions.Controls.Add(lblColour);
            pnlOptions.Controls.Add(buttPurple);
            pnlOptions.Controls.Add(buttBlue);
            pnlOptions.Controls.Add(buttGreen);
            pnlOptions.Controls.Add(buttRed);
            pnlOptions.Controls.Add(buttOrange);
            pnlOptions.Controls.Add(buttRandom);
            pnlOptions.Controls.Add(lblDebug);
            pnlOptions.Controls.Add(chkDebug);
            pnlOptions.Controls.Add(buttBack);

            //set default selected buttons to match current settings
            RefreshOptionSelections();
        }

        //creates a main menu button (play, options, quit)
        private Button CreateMenuButton(string text, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Consolas", 16, FontStyle.Bold),
                ForeColor = colText,
                BackColor = colButton,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 55),
                Location = location,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderColor = colBorder;
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.MouseOverBackColor = colHover;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 50, 50);
            return btn;
        }

        //creates a small option toggle button 
        // with fixed numbers and parameters
        private Button CreateOptionButton(string text, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Consolas", 11, FontStyle.Regular),
                ForeColor = colText,
                BackColor = colButton,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 38),
                Location = location,
                Cursor = Cursors.Hand   //pass in cursor so we can check hover
            };
            //border config
            btn.FlatAppearance.BorderColor = colBorder;
            btn.FlatAppearance.BorderSize = 1;

            //colour for when mouse hovers over
            btn.FlatAppearance.MouseOverBackColor = colHover;

            return btn;
        }

        //creates button with custom colour dependant on parameters entered
        private Button CreateColourButton(string text, Point location, Color tint)
        {
            //initializes button oon pre-fixed size and variables entered, also passes in cursor obj for hover
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Consolas", 10, FontStyle.Regular),
                ForeColor = Color.White,
                BackColor = tint,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 36),
                Location = location,
                Cursor = Cursors.Hand
            };

            //border colour
            btn.FlatAppearance.BorderColor = colBorder;

            //border thickness
            btn.FlatAppearance.BorderSize = 1;

            //lightens colour
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(tint);

            return btn;
        }

        //creates a section label using parameters entered, simplifies creation of label obj
        private Label CreateSectionLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Consolas", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 160, 160),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = location
            };
        }

        //saves selected difficulty and handles highlighting
        private void SelectDifficulty(Button btn, GameSettings.DifficultyLevel level)
        {
            //reset the previously selecteddifficulty colour
            if (selectedDifficulty != null)
            {
                selectedDifficulty.BackColor = colButton;
                selectedDifficulty.ForeColor = colText;
                selectedDifficulty.FlatAppearance.BorderColor = colBorder;
            }

            //highlight currently selected difficulty
            selectedDifficulty = btn;
            btn.BackColor = colAccent;
            btn.ForeColor = Color.Yellow;
            btn.FlatAppearance.BorderColor = colSelected;

            //sets the difficulty according to parameter of difficulty passed in
            GameSettings.Difficulty = level;
        }

        //highlights the selected maze size button and saves to settings
        private void SelectSize(Button btn, GameSettings.MazeSize size)
        {
            if (selectedSize != null)
            {
                selectedSize.BackColor = colButton;
                selectedSize.ForeColor = colText;
                selectedSize.FlatAppearance.BorderColor = colBorder;
            }
            selectedSize = btn;
            btn.BackColor = colAccent;
            btn.ForeColor = Color.Yellow;
            btn.FlatAppearance.BorderColor = colSelected;
            GameSettings.Size = size;
        }

        //highlights the selected colour button and saves to settings
        private void SelectColour(Button btn, GameSettings.ColourPreset preset)
        {
            if (selectedColour != null)
                selectedColour.FlatAppearance.BorderColor = colBorder;

            selectedColour = btn;
            btn.FlatAppearance.BorderColor = Color.Yellow;
            GameSettings.WallColour = preset;
        }

        //sets button highlight states to match whichever settings are currently active
        private void RefreshOptionSelections()
        {
            Button diffBtn = GameSettings.Difficulty switch
            {
                GameSettings.DifficultyLevel.Easy => buttEasy,
                GameSettings.DifficultyLevel.Hard => buttHard,
                _ => buttNormal
            };
            SelectDifficulty(diffBtn, GameSettings.Difficulty);

            Button sizeBtn = GameSettings.Size switch
            {
                GameSettings.MazeSize.Small => buttSmall,
                GameSettings.MazeSize.Large => buttLarge,
                _ => buttMedium
            };
            SelectSize(sizeBtn, GameSettings.Size);

            Button colBtn = GameSettings.WallColour switch
            {
                GameSettings.ColourPreset.Blue => buttBlue,
                GameSettings.ColourPreset.Green => buttGreen,
                GameSettings.ColourPreset.Red => buttRed,
                GameSettings.ColourPreset.Orange => buttOrange,
                GameSettings.ColourPreset.Random => buttRandom,

                //failsafe
                _ => buttRandom
            };
            SelectColour(colBtn, GameSettings.WallColour);

            chkDebug.Checked = GameSettings.ShowDebugPanel;
        }

        //switches to the options panel
        private void ShowOptions()
        {
            RefreshOptionSelections();
            pnlMain.Visible = false;
            pnlOptions.Visible = true;
        }

        //switches back to the main panel
        private void ShowMain()
        {
            pnlOptions.Visible = false;
            pnlMain.Visible = true;
        }

        //closes the menu form and starts the game
        private void StartGame()
        {
            this.Hide();
            Form1 game = new Form1();   //create new instance of game form
            game.FormClosed += (s, e) => Application.Exit();    //new form inherits exit, ensures game form when closed exits fully
            game.Show();
        }

        //paints the border for the menu
        private void PaintMenuBorder(object? sender, PaintEventArgs e)
        {
            //border closer than form edge, adds depth to UI
            using Pen borderPen = new Pen(colAccent, 3);
            e.Graphics.DrawRectangle(borderPen, 10, 10, this.ClientSize.Width - 22, this.ClientSize.Height - 22);

            //thinner inner line for layered perspective
            using Pen innerPen = new Pen(Color.FromArgb(60, 255, 255, 255), 1);
            e.Graphics.DrawRectangle(innerPen, 14, 14, this.ClientSize.Width - 30, this.ClientSize.Height - 30);
        }
    }
}