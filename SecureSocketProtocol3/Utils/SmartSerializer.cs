﻿using SecureSocketProtocol3.Compressions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

/*
    The MIT License (MIT)

    Copyright (c) 2016 AnguisCaptor

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

namespace SecureSocketProtocol3.Utils
{
    public class SmartSerializer
    {
        private enum ObjectTypes : byte
        {
            Null = 0,
            Byte = 1,
            ByteArray,
            Short,
            UShort,
            Int,
            UINT,
            Long,
            Bool,
            String,
            SolidBrush,
            Other,
            Rectangle,
            Size,
            Bitmap,
        };

        private LzwCompression lzwCompression;

        public SmartSerializer()
        {
            this.lzwCompression = new LzwCompression(80);
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
                return new byte[] { (byte)ObjectTypes.Null };

            PayloadWriter pw = new PayloadWriter();
            Type ObjectType = obj.GetType();

            if (ObjectType == typeof(byte))
            {
                pw.WriteByte((byte)ObjectTypes.Byte);
                pw.WriteByte((byte)obj);
            }
            else if (ObjectType == typeof(byte[]))
            {
                byte[] data = (byte[])obj;
                pw.WriteByte((byte)ObjectTypes.ByteArray);
                pw.WriteInteger(data.Length);
                pw.WriteBytes(data);
            }
            else if (ObjectType == typeof(short))
            {
                pw.WriteByte((byte)ObjectTypes.Short);
                pw.WriteShort((short)obj);
            }
            else if (ObjectType == typeof(ushort))
            {
                pw.WriteByte((byte)ObjectTypes.UShort);
                pw.WriteUShort((ushort)obj);
            }
            else if (ObjectType == typeof(int))
            {
                pw.WriteByte((byte)ObjectTypes.Int);
                pw.WriteInteger((int)obj);
            }
            else if (ObjectType == typeof(uint))
            {
                pw.WriteByte((byte)ObjectTypes.UINT);
                pw.WriteUInteger((uint)obj);
            }
            else if (ObjectType == typeof(long))
            {
                pw.WriteByte((byte)ObjectTypes.Long);
                pw.WriteLong((long)obj);
            }
            else if (ObjectType == typeof(bool))
            {
                pw.WriteByte((byte)ObjectTypes.Bool);
                pw.WriteByte((bool)obj ? (byte)1 : (byte)0);
            }
            else if (ObjectType == typeof(string))
            {
                pw.WriteByte((byte)ObjectTypes.String);
                pw.WriteString(obj.ToString());
            }
            else if (ObjectType == typeof(SolidBrush))
            {
                pw.WriteByte((byte)ObjectTypes.SolidBrush);
                pw.WriteByte(((SolidBrush)obj).Color.A);
                pw.WriteByte(((SolidBrush)obj).Color.R);
                pw.WriteByte(((SolidBrush)obj).Color.G);
                pw.WriteByte(((SolidBrush)obj).Color.B);
            }
            else if (ObjectType == typeof(Rectangle))
            {
                pw.WriteByte((byte)ObjectTypes.Rectangle);
                pw.WriteInteger(((Rectangle)obj).X);
                pw.WriteInteger(((Rectangle)obj).Y);
                pw.WriteInteger(((Rectangle)obj).Width);
                pw.WriteInteger(((Rectangle)obj).Height);
            }
            else if (ObjectType == typeof(Size))
            {
                pw.WriteByte((byte)ObjectTypes.Size);
                pw.WriteInteger(((Size)obj).Width);
                pw.WriteInteger(((Size)obj).Height);
            }
            else if (ObjectType == typeof(Bitmap) || ObjectType == typeof(Image))
            {
                pw.WriteByte((byte)ObjectTypes.Bitmap);
                lzwCompression.Compress((Bitmap)obj, pw.vStream);
            }
            else
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                pw.WriteByte((byte)ObjectTypes.Other);
                pw.WriteBytes(ms.GetBuffer(), 0, (int)ms.Length);
                ms.Close();
            }
            return pw.ToByteArray();
        }

        public object Deserialize(byte[] data)
        {
            PayloadReader pr = new PayloadReader(data);
            ObjectTypes Id = (ObjectTypes)pr.ReadByte();

            switch (Id)
            {
                case ObjectTypes.Null: return null;
                case ObjectTypes.Other:
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Binder = new DeserializationBinder();
                    return bf.Deserialize(new MemoryStream(pr.ReadBytes(pr.Length - pr.Position)));
                }
                case ObjectTypes.Byte: return pr.ReadByte();
                case ObjectTypes.Short: return pr.ReadShort();
                case ObjectTypes.UShort: return pr.ReadUShort();
                case ObjectTypes.Int: return pr.ReadInteger();
                case ObjectTypes.UINT: return pr.ReadUInteger();
                case ObjectTypes.Long: return pr.ReadLong();
                case ObjectTypes.Bool: return pr.ReadByte() == 1;
                case ObjectTypes.String: return pr.ReadString();
                case ObjectTypes.SolidBrush: return new SolidBrush(Color.FromArgb(pr.ReadByte(), pr.ReadByte(), pr.ReadByte(), pr.ReadByte()));
                case ObjectTypes.Rectangle: return new Rectangle(pr.ReadInteger(), pr.ReadInteger(), pr.ReadInteger(), pr.ReadInteger());
                case ObjectTypes.Size: return new Size(pr.ReadInteger(), pr.ReadInteger());
                case ObjectTypes.ByteArray: return pr.ReadBytes(pr.ReadInteger());
                case ObjectTypes.Bitmap:
                {
                    using (MemoryStream stream = new MemoryStream(pr.Buffer, pr.Position, pr.Length - pr.Position))
                    {
                        long oldPos = stream.Position;
                        Bitmap bmp = (Bitmap)Bitmap.FromStream(stream);
                        pr.Position += (int)(stream.Position - oldPos);
                        return bmp;
                    }
                }
                default: throw new Exception("Error deserializing");
            }
        }

        //Thanks Banksy
        private class DeserializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;

                // For each assemblyName/typeName that you want to deserialize to
                // a different type, set typeToDeserialize to the desired type.
                String exeAssembly = Assembly.GetExecutingAssembly().FullName;

                // The following line of code returns the type.
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                    typeName, exeAssembly));

                return typeToDeserialize;
            }
        }
    }
}