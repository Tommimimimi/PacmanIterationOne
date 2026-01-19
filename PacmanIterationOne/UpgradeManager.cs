using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pIterationOne
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    //manages the upgrade pool, rolls choices, and applies the selected upgrade
    public class UpgradeManager
    {
        //stores all upgrades in game, current choice of upgrades, and the picked ones in the run.
        private readonly List<Upgrade> pool = new();
        private readonly List<Upgrade> currentChoices = new();
        private readonly List<Upgrade> picked = new();
        private readonly Random rnd = new();

        //how many choices to show each time
        public int ChoiceCount { get; set; } = 3;
          
        //read only access for ui
        public IReadOnlyList<Upgrade> CurrentChoices => currentChoices;
        public IReadOnlyList<Upgrade> PickedUpgrades => picked;

        //add upgrades into the pool from your main form init
        public void Register(Upgrade upgrade)
        {
            if (upgrade == null) 
                return;
            pool.Add(upgrade);
        }

        //optional helper to register many at once
        public void RegisterRange(IEnumerable<Upgrade> upgrades)
        {
            if (upgrades == null)
                return;
            foreach (var u in upgrades) Register(u);
        }

        //rolls choices (call when maze is cleared)
        public void RollChoices()
        {
            //refresh list
            currentChoices.Clear();

            //filter upgrades that can still be applied (stacking rules live in upgrade.TryApply)
            var candidates = pool.Where(CanStillOffer).ToList();

            //if you have fewer candidates than choice count, just show what you have
            Shuffle(candidates);

            int take = Math.Min(ChoiceCount, candidates.Count);
            for (int i = 0; i < take; i++)
                currentChoices.Add(candidates[i]);
        }

        //applies the chosen upgrade by index (call from button click)
        public bool Pick(int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex >= currentChoices.Count)
                return false;

            var selected = currentChoices[choiceIndex];

            //attempt apply (enforces stacking / max stacks)
            bool applied = selected.TryApply();
            if (!applied) return false;

            picked.Add(selected);

            //if it can no longer be applied, stop offering it
            if (!CanStillOffer(selected))
                pool.Remove(selected);

            currentChoices.Clear();
            return true;
        }

        //basic offer rule based on stacks of upgrades
        private static bool CanStillOffer(Upgrade upgrade)
        {
            if (!upgrade.Stackable) return upgrade.CurrentStacks < 1;
            return upgrade.CurrentStacks < upgrade.MaxStacks;
        }

        private void Shuffle<T>(IList<T> list)
        {
            //basic shuffle
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

}
