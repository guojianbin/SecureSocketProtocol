﻿using SecureSocketProtocol3.Network;
using SecureSocketProtocol3.Security.Encryptions;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SecureSocketProtocol3.Security.Layers
{
    public class AesLayer : ILayer
    {
        private HwAes EncAES;
        private Connection connection;

        public AesLayer(Connection connection)
        {
            this.connection = connection;
            this.EncAES = new HwAes(connection, connection.NetworkKey, 256, CipherMode.CBC, PaddingMode.PKCS7);
        }

        public LayerType Type
        {
            get
            {
                return LayerType.Encryption;
            }
        }

        public void ApplyKey(byte[] Key, byte[] Salt)
        {
            //this.EncAES.Key = Key;
        }

        public void ApplyLayer(byte[] InData, int InOffset, int InLen, ref byte[] OutData, ref int OutOffset, ref int OutLen)
        {
            OutData = EncAES.Encrypt(InData, InOffset, InLen);
            OutOffset = 0;
            OutLen = OutData.Length;
        }

        public void RemoveLayer(byte[] InData, int InOffset, int InLen, ref byte[] OutData, ref int OutOffset, ref int OutLen)
        {
            OutData = EncAES.Decrypt(InData, InOffset, InLen);
            OutOffset = 0;
            OutLen = OutData.Length;
        }
    }
}