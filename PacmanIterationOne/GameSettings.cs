using System;
using System.Drawing;

namespace pIterationOne
{
    //holds all player-configurable settings shared between the menu and game
    public static class GameSettings
    {
        //affects ghost speed multiplier and phase timings
        public enum DifficultyLevel
        {
            Easy,
            Normal,
            Hard
        }

        //maze size enum for preset values to be used for random uneven ranges
        public enum MazeSize
        {
            Small,
            Medium,
            Large
        }

        //colour presets for maze walls
        public enum ColourPreset
        {
            Purple,
            Blue,
            Green,
            Red,
            Orange,
            Random
        }

        //defaults set to normal so game works at intended level if untouched
        public static DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;
        public static MazeSize Size { get; set; } = MazeSize.Medium;
        public static bool ShowDebugPanel { get; set; } = false;
        public static ColourPreset WallColour { get; set; } = ColourPreset.Random;

        //returns ghost speed mult based on difficulty selected
        public static float GetGhostSpeedMultiplier()
        {
            return Difficulty switch
            {
                DifficultyLevel.Easy   => 0.7f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard   => 1.3f,
                _ => 1.0f
            };
        }

        //returns chase seconds based on difficulty
        //harder difficulties -> ghosts chase for longer before scattering
        public static int GetChaseSeconds()
        {
            return Difficulty switch
            {
                DifficultyLevel.Easy   => 5,
                DifficultyLevel.Normal => 7,
                DifficultyLevel.Hard   => 12,
                _ => 7
            };
        }

        //returns scatter seconds based on difficulty
        //easier difficulties -> ghosts scatter for longer
        public static int GetScatterSeconds()
        {
            return Difficulty switch
            {
                DifficultyLevel.Easy   => 20,
                DifficultyLevel.Normal => 15,
                DifficultyLevel.Hard   => 8,
                _ => 15
            };
        }

        //returns the min max rand values for maze x based on chosen size
        public static (int min, int max) GetMazeXRange()
        {
            return Size switch
            {
                MazeSize.Small  => (4, 6),
                MazeSize.Medium => (6, 8),
                MazeSize.Large  => (8, 12),
                _ => (6, 8)
            };
        }

        //returns the min max rand values for maze y based on chosen size
        public static (int min, int max) GetMazeYRange()
        {
            return Size switch
            {
                MazeSize.Small  => (8, 11),
                MazeSize.Medium => (12, 16),
                MazeSize.Large  => (18, 22),
                _ => (12, 16)
            };
        }

        //returns wall brush colour based on the chosen preset
        //random uses the same rnd system
        public static Color GetWallColour(Random rnd)
        {
            return WallColour switch
            {
                ColourPreset.Purple => Color.FromArgb(255, 60,  5,  60),    //deep purple
                ColourPreset.Blue   => Color.FromArgb(255, 23,  23,  164),  //original game blue wall colour
                ColourPreset.Green  => Color.FromArgb(220, 7,  80,  15),    //medium dark green
                ColourPreset.Red    => Color.FromArgb(220, 175,  10,  15),  //medium red 
                ColourPreset.Orange => Color.FromArgb(220, 155, 75,  5),    //tangerine orange

                ColourPreset.Random => Color.FromArgb(220, rnd.Next(20, 100), rnd.Next(0, 15), rnd.Next(20, 100)),
                _ => Color.FromArgb(220, 80, 80, 80)    //failsafe to always return value, returns medium grey
            };
        }
    }
}