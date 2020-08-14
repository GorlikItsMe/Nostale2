using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using NosCore.Packets;
using NosCore.Packets.Interfaces;
using NosCore.Packets.ServerPackets.CharacterSelectionScreen;
using NosCore.Packets.ServerPackets.Login;
using NosCore.Packets.ServerPackets.MiniMap;
using NosCore.Packets.ServerPackets.Visibility;
using NosCore.Packets.Enumerations;


namespace Nostale2.PacketHandler
{
    public class PacketHandler
    {
        IDeserializer Deserializer;
        NostaleMain nt;
        public PacketHandler()
        {
            Deserializer = new Deserializer(new[] { 
                typeof(AtPacket),
                typeof(InPacket)
            });
        }
        public void Setup()
        {
            nt = GameObject.Find("NostaleMain").GetComponent<NostaleMain>();
        }
        public void Handle(string packet_raw)
        {
            string h = packet_raw.Split(' ')[0];
            switch (h)
            {
                case "at":
                    AtPacket p = (AtPacket)Deserializer.Deserialize(packet_raw);
                    Debug.Log("Load map: " + p.MapId);

                    GameObject player = nt.PlacePlayer(p.PositionX, p.PositionY);

                    PlayerControler pc = player.GetComponent<PlayerControler>();
                    pc.CharacterId = p.CharacterId;
                    break;
                case "in":
                    // p = (InPacket)Deserializer.Deserialize(packet_raw);
                    Debug.Log(packet_raw);
                    //in 2 630 2000095 24 109 2 100 100 0 0 3 37 1 0 -1 Black^Bushi 0 0 0 0 0 0 0 0 0 0

                    // Pakiety wyglądają inaczej. Trzeba naprawić albo własny handler
                    /*InPacketHandler.h( new InPacket
                    {
                        VisualType = VisualType.Npc,
                        Name = packet_raw.Split(' ')[16].Replace('^',' '),
                        VisualId = 0,
                        VNum = packet_raw.Split(' ')[2],
                        PositionX = short.Parse(packet_raw.Split(' ')[4]),
                        PositionY = short.Parse(packet_raw.Split(' ')[5]),
                        Direction = 0,
                        InNonPlayerSubPacket = new InNonPlayerSubPacket
                        {
                            Dialog = 0,
                            InAliveSubPacket = new InAliveSubPacket
                            {
                                Mp = 0,
                                Hp = 0
                            },
                            IsSitting = false,
                            SpawnEffect = SpawnEffectType.NoEffect,
                            Unknow1 = 2
                        }
                    });*/
                    InPacketHandler.h( (InPacket)Deserializer.Deserialize(packet_raw) );

                    break;
                default:
                    if (h != "stat")
                    {
                        Debug.LogWarning(packet_raw);
                    }
                    break;
            }
        }
    }
}
