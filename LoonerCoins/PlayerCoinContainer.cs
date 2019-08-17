using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace paddywan
{
    class PlayerCoinContainer
    {
        public ulong steamID;
        public uint coins;

        public PlayerCoinContainer()
        {
        }

        public PlayerCoinContainer(ulong _steamID, uint _coins)
        {
            this.steamID = _steamID;
            this.coins = _coins;
        }
    }
}
