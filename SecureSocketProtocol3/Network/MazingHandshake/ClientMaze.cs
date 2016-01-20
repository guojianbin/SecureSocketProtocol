﻿using SecureSocketProtocol3.Security.Encryptions;
using SecureSocketProtocol3.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/*
    Secure Socket Protocol
    Copyright (C) 2016 AnguisCaptor

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace SecureSocketProtocol3.Network.MazingHandshake
{
    public class ClientMaze : Mazing
    {
        public WopEx wopEx;

        public ClientMaze(Size size, int MazeCount, int MazeSteps)
            : base(size, MazeCount, MazeSteps)
        {

        }

        public override MazeErrorCode onReceiveData(byte[] Data, ref byte[] ResponseData)
        {
            ResponseData = new byte[0];
            switch (base.Step)
            {
                case 1:
                {
                    if (Data.Length != 32)
                    {
                        SysLogger.Log("[MazeHandShake][Server] Receive Length missmatch", SysLogType.Debug);
                        return MazeErrorCode.Error;
                    }

                    wopEx = base.GetWopEncryption();
                    wopEx.Decrypt(Data, 0, Data.Length, new MemoryStream(Data));

                    BigInteger server_prime = new BigInteger(Data);
                    if (server_prime.isProbablePrime())
                    {
                        //verify the prime from the server
                        BigInteger server_Prime_test = BigInteger.genPseudoPrime(256, 50, new Random(BitConverter.ToInt32(wopEx.Key, 0)));

                        if (server_prime != server_Prime_test)
                        {
                            //Attacker detected ?
                            SysLogger.Log("[MazeHandShake][Server] Man-In-The-Middle detected", SysLogType.Debug);
                            return MazeErrorCode.Error;
                        }

                        //successful
                        //generate another prime and send it back
                        BigInteger client_Prime = BigInteger.genPseudoPrime(256, 50, new Random(server_prime.IntValue()));

                        byte[] primeData = client_Prime.getBytes();
                        wopEx.Encrypt(primeData, 0, primeData.Length, new MemoryStream(primeData));
                        ResponseData = primeData;

                        BigInteger key = base.ModKey(server_prime, client_Prime);


                        //apply key to encryption
                        ApplyKey(wopEx, key);

                        base.FinalKey = wopEx.Key;
                        base.FinalSalt = wopEx.Salt;

                        Step++;
                        return MazeErrorCode.Finished;
                    }
                    else
                    {
                        //connection failed, using old keys ?
                        SysLogger.Log("[MazeHandShake][Server] Invalid received data", SysLogType.Debug);
                        return MazeErrorCode.Error;
                    }
                }
            }

            return MazeErrorCode.Success;
        }
    }
}