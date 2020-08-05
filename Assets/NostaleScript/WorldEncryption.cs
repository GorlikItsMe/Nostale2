using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public sealed class WorldEncryption
    {
        private static readonly char[] Keys = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };
        private byte[] notFullPacket = { };
        public int EncryptionKey { get; }

        /// <summary>
        /// Create a new WorldEncryption instance
        /// </summary>
        /// <param name="encryptionKey">Encryption key received by LoginServer</param>
        public WorldEncryption(int encryptionKey)
        {
            EncryptionKey = encryptionKey;
            
        }

        /// <summary>
        ///     Decrypt the raw packet (byte array) to a readable list string
        /// </summary>
        /// <param name="bytes">Bytes to decrypt</param>
        /// <param name="size">Amount of byte to read</param>
        /// <returns>Decrypted packet to string list</returns>
        public List<string> Decrypt(byte[] bytes, int size)
        {
            var output = new List<string>();
            //bool debugPackets = false;
            if(notFullPacket.Length != 0)
            {
                byte[] _temp = new byte[notFullPacket.Length + bytes.Length];
                notFullPacket.CopyTo(_temp, 0);
                bytes.CopyTo(_temp, notFullPacket.Length);
                bytes = _temp;
                size = bytes.Length;
                notFullPacket = new byte[] { };
                //Console.WriteLine("Łącze 2 pakiety w jeden! L: {0}", bytes.Length);
                //debugPackets = false;
            }

            var currentPacket = "";
            byte[] currentPacketb = { };
            var index = 0;

            while (index < size)
            {
                byte currentByte = bytes[index];
                index++;

                if (currentByte == 0xFF)
                {
                    /*if (debugPackets)
                    {
                        Console.WriteLine("Debug");
                        Console.WriteLine(Encoding.UTF8.GetString(currentPacketb));
                        Console.WriteLine("Debug");
                        debugPackets = false;
                    }*/
                    
                    output.Add(System.Text.Encoding.GetEncoding(1250).GetString(currentPacketb)); // PL ENCODEING
                    //output.Add(Encoding.UTF8.GetString(currentPacketb));
                    //output.Add(currentPacket);
                    currentPacket = "";
                    currentPacketb = new byte[] { };
                    continue;
                }

                var length = (byte)(currentByte & 0x7F);

                if ((currentByte & 0x80) != 0)
                {
                    while (length != 0)
                    {
                        if (index < size) // wywaliłem <= na < bo wywalało index error
                        {
                            currentByte = bytes[index];
                            index++;

                            var firstIndex = (byte)(((currentByte & 0xF0u) >> 4) - 1);
                            var first = (byte)(firstIndex != 255 ? firstIndex != 14 ? Keys[firstIndex] : '\u0000' : '?');
                            if (first != 0x6E)
                            {
                                currentPacketb = addByteToArray(currentPacketb, first);
                                currentPacket += Convert.ToChar(first);
                            }
                            if (length <= 1)
                                break;

                            var secondIndex = (byte)((currentByte & 0xF) - 1);
                            var second = (byte)(secondIndex != 255 ? secondIndex != 14 ? Keys[secondIndex] : '\u0000' : '?');
                            if (second != 0x6E)
                            {
                                currentPacket += Convert.ToChar(second);
                                currentPacketb = addByteToArray(currentPacketb, second);
                            }

                            length -= 2;
                        }
                        else
                        {
                            length--;
                        }
                    }
                }
                else
                {
                    while (length != 0)
                    {
                        if (index < size)
                        {
                            currentPacket += Convert.ToChar(bytes[index] ^ 0xFF);
                            currentPacketb = addByteToArray(currentPacketb, Convert.ToByte(bytes[index] ^ 0xFF));
                            index++;
                        }
                        else if (index == size)
                        {
                            currentPacket += Convert.ToChar(0xFF);
                            currentPacketb = addByteToArray(currentPacketb, Convert.ToByte(0xFF));
                            index++;
                        }

                        length--;
                    }
                }
            }

            if (currentPacketb.Length != 0) // cos zostało
            {
                notFullPacket = bytes; //zapisz
                //Console.WriteLine("USZKODZONY PAKIET CZEKAM NA RESZTE L: {0}, Bytes.Lenght: {1} / {2}", output.Count, bytes.Length, size);
                //Console.WriteLine(Encoding.UTF8.GetString(currentPacketb));
                return new List<string>();
                //Console.WriteLine("FORCE BROKEN PACKET");
                //output.Add(Encoding.UTF8.GetString(currentPacketb));
            }
            
            return output;
        }

        public byte[] addByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 0);
            newArray[bArray.Length] = newByte;
            return newArray;
        }

        /// <summary>
        ///     Encrypt the string packet to byte array
        /// </summary>
        /// <param name="value">String to encrypt</param>
        /// <param name="session">Define if it's a session packet or not</param>
        /// <returns>Encrypted packet as byte array</returns>
        public byte[] Encrypt(string value, bool session = false)
        {
            var output = new List<byte>();

            var mask = new string(value.Select(c =>
            {
                var b = (sbyte)c;
                if (c == '#' || c == '/' || c == '%')
                    return '0';
                if ((b -= 0x20) == 0 || (b += unchecked((sbyte)0xF1)) < 0 || (b -= 0xB) < 0 ||
                    b - unchecked((sbyte)0xC5) == 0)
                    return '1';
                return '0';
            }).ToArray());

            int packetLength = value.Length;

            var sequenceCounter = 0;
            var currentPosition = 0;

            while (currentPosition <= packetLength)
            {
                int lastPosition = currentPosition;
                while (currentPosition < packetLength && mask[currentPosition] == '0')
                    currentPosition++;

                int sequences;
                int length;

                if (currentPosition != 0)
                {
                    length = currentPosition - lastPosition;
                    sequences = length / 0x7E;
                    for (var i = 0; i < length; i++, lastPosition++)
                    {
                        if (i == sequenceCounter * 0x7E)
                        {
                            if (sequences == 0)
                            {
                                output.Add((byte)(length - i));
                            }
                            else
                            {
                                output.Add(0x7E);
                                sequences--;
                                sequenceCounter++;
                            }
                        }

                        output.Add((byte)((byte)value[lastPosition] ^ 0xFF));
                    }
                }

                if (currentPosition >= packetLength)
                    break;

                lastPosition = currentPosition;
                while (currentPosition < packetLength && mask[currentPosition] == '1')
                    currentPosition++;

                if (currentPosition == 0) continue;

                length = currentPosition - lastPosition;
                sequences = length / 0x7E;
                for (var i = 0; i < length; i++, lastPosition++)
                {
                    if (i == sequenceCounter * 0x7E)
                    {
                        if (sequences == 0)
                        {
                            output.Add((byte)((length - i) | 0x80));
                        }
                        else
                        {
                            output.Add(0x7E | 0x80);
                            sequences--;
                            sequenceCounter++;
                        }
                    }

                    var currentByte = (byte)value[lastPosition];
                    switch (currentByte)
                    {
                        case 0x20:
                            currentByte = 1;
                            break;
                        case 0x2D:
                            currentByte = 2;
                            break;
                        case 0xFF:
                            currentByte = 0xE;
                            break;
                        default:
                            currentByte -= 0x2C;
                            break;
                    }

                    if (currentByte == 0x00) continue;

                    if (i % 2 == 0)
                        output.Add((byte)(currentByte << 4));
                    else
                        output[output.Count - 1] = (byte)(output.Last() | currentByte);
                }
            }

            output.Add(0xFF);

            var sessionNumber = (sbyte)((EncryptionKey >> 6) & 0xFF & 0x80000003);

            if (sessionNumber < 0)
                sessionNumber = (sbyte)(((sessionNumber - 1) | 0xFFFFFFFC) + 1);

            var sessionKey = (byte)(EncryptionKey & 0xFF);

            if (session)
                sessionNumber = -1;

            switch (sessionNumber)
            {
                case 0:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)(output[i] + sessionKey + 0x40);
                    break;
                case 1:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)(output[i] - (sessionKey + 0x40));
                    break;
                case 2:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)((output[i] ^ 0xC3) + sessionKey + 0x40);
                    break;
                case 3:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)((output[i] ^ 0xC3) - (sessionKey + 0x40));
                    break;
                default:
                    for (var i = 0; i < output.Count; i++)
                        output[i] = (byte)(output[i] + 0x0F);
                    break;
            }

            return output.ToArray();
        }

    }
