using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NosCore.Packets.ServerPackets.Visibility;
using NosCore.Packets.Enumerations;

namespace Nostale2.PacketHandler
{
    class InPacketHandler
    {
        static public void h(InPacket p)
        {
            if(p.VisualType == VisualType.Npc)
            {
                var position = new Vector3(p.PositionX, 0, p.PositionY);
                Quaternion rotation = Quaternion.identity;
                GameObject obj = Resources.Load("Prefabs/NPC") as GameObject;
                UnityEngine.Object.Instantiate(obj, position, rotation);
                return;
            }
        }
    }
}
