using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pIterationOne
{
    //defines all possible roguelike upgrade types used in the game
    public enum UpgradeType
    {
        //TODO: CONFIGURE AND FINALISE ABILITIES WITH BALANCING

        //movement and control upgrades:
        sprint,        //slightly increases player movement speed
        dash,              //allows a short burst of movement on demand

        //survivability upgrades:
        shield,            //blocks the first ghost collision per maze
        extralife,         //grants an additional life for the run

        //ghost interaction upgrades:
        slowghost,         //reduces ghost movement speed
        phasedelay,        //extend scatter duration

        //scoring and risk reward upgrades:
        scoremult,  //increases score gained from pellets
        greed,             //higher score however there are faster ghosts

        //utility and control upgrades:
        magnet,     //wider range for pellet pickup
        lastchance        //prevents death once per run
    }

    public class Upgrade
    {
        //core identity and ui information
        public UpgradeType Type { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }

        //stacking rules and tracking
        public bool Stackable { get; init; }
        public int MaxStacks { get; init; }
        public int CurrentStacks { get; private set; }

        //logic that will help apply the upgrade effect to the player and ghosts
        public Action Apply { get; }

        //constructor used to initialise all upgrade properties
        public Upgrade(
            UpgradeType type,
            string name,
            string description,
            Action apply,
            bool stackable = false,
            int maxStacks = 1
        )
        {
            Type = type;
            Name = name;
            Description = description;
            Apply = apply;

            Stackable = stackable;
            MaxStacks = maxStacks;
            CurrentStacks = 0;
        }

        //attempts to apply the upgrade while enforcing stacking rules
        public bool TryApply()
        {
            if (!Stackable && CurrentStacks >= 1)
                return false;

            if (Stackable && CurrentStacks >= MaxStacks)
                return false;

            CurrentStacks++;
            Apply();
            return true;
        }
    }
}
