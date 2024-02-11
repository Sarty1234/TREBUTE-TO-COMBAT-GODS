using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game.Other
{
    public class PlayerHP : IHPScript
    {
        public override void Dead()
        {
            if (!IsOwner) return;
            base.Dead();


            GetComponent<PlayerData>().Dead();
        }
    }
}
