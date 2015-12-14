﻿using SecureSocketProtocol3.Security.Encryptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SecureSocketProtocol3.Utils
{
    /// <summary>
    /// RandomEx is a experimental Random class
    /// </summary>
    public class RandomEx
    {
        //By using WopEx we could technically could encrypt/decrypt the random seed we get by Calling GetNext, maybe a fun idea for in the future
        private WopEx wopEx;
        private byte[] encryptCode;
        private byte[] decryptCode;
        private byte[] IntData;

        public RandomEx()
        {
            Random rnd = new Random();

            // test/generate the random algorithm
            while (true)
            {
                List<int> temp = new List<int>();
                IntData = BitConverter.GetBytes(rnd.Next());
                WopEx.GenerateCryptoCode(rnd.Next(), 10, ref encryptCode, ref decryptCode);
                this.wopEx = new WopEx(BitConverter.GetBytes(rnd.Next()), BitConverter.GetBytes(rnd.Next()), DateTime.Now.Millisecond, encryptCode, decryptCode, WopEncMode.ShuffleInstructions, 2, false);
                bool success = true;

                for (int i = 0; i < 100; i++)
                {
                    int tmpInt = GetNext();
                    if (!temp.Contains(tmpInt))
                        temp.Add(tmpInt);
                    else
                    {
                        success = false;
                        break;
                    }
                }
                if (success)
                    break;
                temp.Clear();
            }
        }

        public int GetNext(int max)
        {
            return GetNext() & max;
        }
        public int GetNext(int min, int max)
        {
            int rnd = GetNext();

            while(rnd < min || rnd > max)
                rnd = GetNext();

            return rnd;
        }

        public int GetNext()
        {
            wopEx.Encrypt(IntData, 0, 4, new MemoryStream(IntData));
            return BitConverter.ToInt32(IntData, 0);
        }

        public uint GetUNext()
        {
            wopEx.Encrypt(IntData, 0, 4, new MemoryStream(IntData));
            return BitConverter.ToUInt32(IntData, 0);
        }
    }
}