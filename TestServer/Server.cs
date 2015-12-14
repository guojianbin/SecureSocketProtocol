﻿using SecureSocketProtocol3;
using SecureSocketProtocol3.Network;
using SecureSocketProtocol3.Security.DataIntegrity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestServer
{
    public class Server : SSPServer
    {
        public Server()
            : base(new ServerProps())
        {

        }

        public override SSPClient GetNewClient()
        {
            //register users if there aren't any, please use a database and not this way
            if (Program.Users.Count == 0)
            {
                List<Stream> keys = new List<Stream>();
                keys.Add(new MemoryStream(File.ReadAllBytes(@".\Data\PrivateKey1.dat")));
                keys.Add(new MemoryStream(File.ReadAllBytes(@".\Data\PrivateKey2.dat")));
                User user = base.RegisterUser("UserTest", "PassTest", keys, new MemoryStream(File.ReadAllBytes(@".\Data\PublicKey1.dat")));

                Program.Users.Add(user.EncryptedHash, user.GetUserDbInfo());
            }
            return new Peer();
        }

        public override User.UserDbInfo onFindUser(string EncryptedPublicKeyHash)
        {
            if (Program.Users.ContainsKey(EncryptedPublicKeyHash))
                return Program.Users[EncryptedPublicKeyHash];
            return null;
        }

        private class ServerProps : ServerProperties
        {

            public override ushort ListenPort
            {
                get { return 444; }
            }

            public override string ListenIp
            {
                get { return "0.0.0.0"; }
            }

            public override string ListenIp6
            {
                get { return "::1"; }
            }

            public override bool UseIPv4AndIPv6
            {
                get { return true; }
            }

            public override Stream[] KeyFiles
            {
                get { return new Stream[0]; }
            }

            public override System.Drawing.Size Handshake_Maze_Size
            {
                get { return new System.Drawing.Size(128, 128); }
            }

            public override ushort Handshake_StepSize
            {
                get { return 5; }
            }

            public override ushort Handshake_MazeCount
            {
                get { return 1; }
            }

            public override byte[] NetworkKey
            {
                get
                {
                    return new byte[]
                    {
                        80, 118, 131, 114, 195, 224, 157, 246, 141, 113,
                        186, 243, 77, 151, 247, 84, 70, 172, 112, 115,
                        112, 110, 91, 212, 159, 147, 180, 188, 143, 251,
                        218, 155
                    };
                }
            }

            public override TimeSpan ClientTimeConnected
            {
                get { return new TimeSpan(1, 0, 0, 0); }
            }
        }
    }
}