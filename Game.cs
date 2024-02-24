using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSDeaths
{
    internal class Game
    {
        public readonly string name;
        public readonly int[] offsets32;
        public readonly int[] offsets64;

        public Game(in string name, in int[] offsets32, in int[] offsets64)
        {
            this.name = name;
            this.offsets32 = offsets32;
            this.offsets64 = offsets64;
        }
    }
}
