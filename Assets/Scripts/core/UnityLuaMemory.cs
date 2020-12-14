using System.Collections;
using UnityEngine;
using System.IO;
using System;
using System.Text;

namespace UnityLua
{
    class UnityLuaMemory : MemoryStream
    {
        //byt 0-255 转化不了就往回转化

        //byte

        #region Short
        /// <summary>
        /// 短整星2个字节
        /// </summary>
        public short ReadShort()
        {
            byte[] arr = new byte[2];
            base.Read(arr, 0, 2);
            return BitConverter.ToInt16(arr, 0);
        }

        /// <summary>
        /// 写入short数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteShort(short value) {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion
        #region ushort
        /// <summary>
        /// 短整星2个字节
        /// </summary>
        public ushort ReadUShort()
        {
            byte[] arr = new byte[2];
            base.Read(arr, 0, 2);
            return BitConverter.ToUInt16(arr, 0);
        }

        /// <summary>
        /// 写入short数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteUShort(ushort value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region Int
        /// <summary>
        /// 整星4个字节
        /// </summary>
        public int ReadInt()
        {
            byte[] arr = new byte[4];
            base.Read(arr, 0, 4);
            return BitConverter.ToInt32(arr, 0);
        }

        /// <summary>
        /// 写入int数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(int value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion
        #region uint
        /// <summary>
        /// 短整星4个字节
        /// </summary>
        public uint ReadUInt()
        {
            byte[] arr = new byte[4];
            base.Read(arr, 0, 4);
            return BitConverter.ToUInt32(arr, 0);
        }

        /// <summary>
        /// 写入short数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt(uint value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region long
        /// <summary>
        /// 整星8个字节
        /// </summary>
        public long ReadLong()
        {
            byte[] arr = new byte[8];
            base.Read(arr, 0, 8);
            return BitConverter.ToInt64(arr, 0);
        }

        /// <summary>
        /// 写入int数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(long value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion
        #region ulong
        /// <summary>
        /// 短整星8个字节
        /// </summary>
        public ulong ReadULong()
        {
            byte[] arr = new byte[8];
            base.Read(arr, 0, 8);
            return BitConverter.ToUInt64(arr, 0);
        }

        /// <summary>
        /// 写入ulong数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteULong(ulong value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region float
        /// <summary>
        /// 4个字节
        /// </summary>
        /// <returns></returns>
        public float ReadFloat() {
            byte[] arr = new byte[4];
            base.Read(arr, 0, 4);
            return BitConverter.ToSingle(arr, 0);
        }

        public void WriteFloat(float value) {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion
        #region double
        /// <summary>
        /// 8个字节
        /// </summary>
        /// <returns></returns>
        public double ReadDouble() {
            byte[] arr = new byte[8];
            base.Read(arr, 0, 8);
            return BitConverter.ToDouble(arr, 0);
        }

        public void WriteDouble(double value) {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region bool
        /// <summary>
        /// 1個字節
        /// </summary>
        /// <returns></returns>
        public bool ReadBool() {
            return base.ReadByte() == 1;
        }

        public void WriteBool(bool value)
        {
            base.WriteByte((byte)(value == true ? 1 : 0));
        }
        #endregion

        #region string
        public string ReadString()
        {
            ushort len = ReadUShort();
            byte[] arr = new byte[len];
            base.Read(arr, 0, len);
            return Encoding.UTF8.GetString(arr);
        }

        public void WriteString(string value) {
            byte[] arr = Encoding.UTF8.GetBytes(value);
            if (arr.Length > 65536) {
                throw new InvalidCastException(" 字符串過大 ");
            }
            WriteUShort((ushort)arr.Length);
            base.Write(arr, 0, arr.Length);
        }
        #endregion
    }

}
