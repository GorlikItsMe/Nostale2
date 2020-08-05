﻿using System;
using UnityEngine.Windows;

public sealed class LoginEncryption
    {
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);

        private readonly string _dxHash;
        private readonly string _glHash;
        private readonly string _version;

        /// <summary>
        ///     Create a new LoginEncryption instance
        /// </summary>
        /// <param name="dxHash">NostaleClientX.exe hash</param>
        /// <param name="glHash">NostaleClient hash</param>
        /// <param name="version">Version of the client</param>
        public LoginEncryption(string dxHash, string glHash, string version)
        {
            _dxHash = dxHash;
            _glHash = glHash;
            _version = version;
        }

        /// <summary>
        ///     Decrypt the raw packet (byte array) to a readable string
        /// </summary>
        /// <param name="bytes">Bytes to decrypt</param>
        /// <param name="size">Amount of byte to translate</param>
        /// <returns>Decrypted packet as string</returns>
        public string Decrypt(byte[] bytes, int size)
        {
            var output = "";
            for (var i = 0; i < size; i++)
            {
                if(bytes[i] != 0)
                {
                   output += Convert.ToChar(bytes[i] - 0xF);
                }
            }
            return output;
        }

        /// <summary>
        ///     Encrypt the string packet to byte array
        /// </summary>
        /// <param name="value">String to encrypt</param>
        /// <returns>Encrypted packet as byte array</returns>
        public byte[] Encrypt(string value)
        {
            var output = new byte[value.Length + 1];
            for (var i = 0; i < value.Length; i++)
            {
                output[i] = (byte)((value[i] ^ 0xC3) + 0xF);
            }
            output[output.Length - 1] = 0xD8;
            return output;
        }

        /// <summary>
        ///     Create the NoS0577 login packet
        /// </summary>
        /// <param name="SESSION_TOKEN">Value generated by ntAuth library, after the value there are two spaces in the login packet</param>
        /// <param name="INSTALLATION_GUID"> Id that is generated during installation, for login purposes it probably may be random, stored in the windows registry under key name InstallationId in SOFTWARE\\WOW6432Node\\Gameforge4d\\TNTClient\\MainApp</param>
        /// <returns>Packet as string created with the value</returns>
        public string CreateLoginPacket(string SESSION_TOKEN, string INSTALLATION_GUID = "1234-1234-1234-1234")
        {
            string md5Hash = Cryptography.ToMd5(_dxHash.ToUpper() + _glHash.ToUpper());
            //string c = char(0xB);
            string c = "\v";
            return $"NoS0577 {SESSION_TOKEN} {INSTALLATION_GUID} 003662BF 4{c}{_version} 0 {md5Hash}";
        }

    public string CreateLoginPacketOld(string LOGIN, string PASSWORD, string INSTALLATION_GUID = "1234-1234-1234-1234")
    {
        string md5Hash = Cryptography.ToMd5(_dxHash.ToUpper() + _glHash.ToUpper()).ToUpper();
        string PASSWORD_HASH = Cryptography.ToSha512(PASSWORD).ToUpper();
        //string c = char(0xB);
        string c = "\v";
        Random rnd = new Random();
        string r_id = rnd.Next(1000000, 9999999).ToString();
        return $"NoS0575 {r_id} {LOGIN} {PASSWORD_HASH} {INSTALLATION_GUID} 0023D513{c}{_version} 0 {md5Hash}";
    }

    public override string ToString()
        {
            return $"{nameof(_dxHash)}: {_dxHash}, {nameof(_glHash)}: {_glHash}, {nameof(_version)}: {_version}";
        }
    }

