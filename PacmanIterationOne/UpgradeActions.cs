using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacmanIterationOne
{
    public static class UpgradeActions
    {
        public static void ApplySprint(ref int pSpeed)
        {
            //increase player movement speed by 50%
            pSpeed += pSpeed / 2;
        }

    }
}
