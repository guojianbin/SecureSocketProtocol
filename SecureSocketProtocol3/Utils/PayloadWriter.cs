﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SecureSocketProtocol3.Utils
{
    public class PayloadWriter : IDisposable
    {
        public MemoryStream vStream { get; set; }
        public bool AutoDispose = true;

        public PayloadWriter()
        {
            vStream = new MemoryStream();
        }
        public PayloadWriter(MemoryStream stream)
        {
            vStream = stream;
        }

        public void WriteBytes(byte[] value)
        {
            vStream.Write(value, 0, value.Length);
        }

        public void WriteBytes(byte[] value, int Offset, int Length)
        {
            vStream.Write(value, Offset, Length);
        }

        public void WriteInteger(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// A integer with 3 bytes not 4
        /// </summary>
        public void WriteThreeByteInteger(int value)
        {
            WriteByte((byte)value);
            WriteByte((byte)(value >> 8));
            WriteByte((byte)(value >> 16));
        }

        public void WriteUInteger(uint value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteShort(short value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        public void WriteUShort(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        public void WriteULong(ulong value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteByte(byte value)
        {
            vStream.WriteByte(value);
        }

        public void WriteBool(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }

        public void WriteDouble(double value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        public void WriteLong(long value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        public void WriteFloat(float value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        public void WriteDecimal(decimal value)
        {
            BinaryWriter writer = new BinaryWriter(vStream);
            writer.Write(value);
        }

        public void WriteString(string value)
        {
            if (!(value == null))
                WriteBytes(System.Text.Encoding.Unicode.GetBytes(value));
            else
                throw new NullReferenceException("value");
            vStream.WriteByte(0);
            vStream.WriteByte(0);
        }

        public void WriteObject(object obj)
        {
            SmartSerializer serializer = new SmartSerializer();
            byte[] serialized = serializer.Serialize(obj);
            WriteInteger(serialized.Length);
            WriteBytes(serialized);
        }

        public void WriteBigInteger(BigInteger BigInt)
        {
            byte[] temp = BigInt.getBytes();
            WriteByte((byte)temp.Length);
            WriteBytes(temp);
        }

        public byte[] ToByteArray()
        {
            return vStream.ToArray();
        }

        /// <summary> Returns the array of unsigned bytes from which this stream was created. </summary>
        public byte[] GetBuffer()
        {
            return vStream.GetBuffer();
        }

        public long Length
        {
            get { return vStream.Length; }
        }

        public long Position
        {
            get { return vStream.Position; }
            set { vStream.Position = value; }
        }

        public void Dispose()
        {
            if (AutoDispose)
            {
                vStream.Close();
                vStream.Dispose();
                vStream = null;
            }
        }
    }
}
