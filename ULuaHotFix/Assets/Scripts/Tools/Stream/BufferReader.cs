

using System;
using System.IO;

public class BufferReader
{
    private MemoryStream mStream;
    private BinaryReader mReader;

    public MemoryStream Stream
    {
        get
        {
            return this.mStream;
        }
    }

    public int Length
    {
        get
        {
            return (int)this.mStream.Length;
        }
    }

    public int Position
    {
        get
        {
            return (int)this.mStream.Position;
        }
    }

    private string m_BuffName = "";

    //buffName用来错误提示
    public BufferReader(byte[] data, string buffName = "")
    {
        this.mStream = new MemoryStream(data);
        this.mReader = new BinaryReader(this.mStream);
        m_BuffName = buffName;
    }

    public BufferReader(byte[] data, int index, int size, string buffName = "")
    {
        this.mStream = new MemoryStream(data, index, size);
        this.mReader = new BinaryReader(this.mStream);
        m_BuffName = buffName;
    }

    public void Read(ref sbyte value)
    {
        value = (sbyte)this.mStream.ReadByte();
    }

    public sbyte ReadSByte()
    {
        return (sbyte)this.mStream.ReadByte();
    }

    public void Read(ref byte value)
    {
        value = (byte)this.mStream.ReadByte();      
    }

    public byte ReadByte()
    {
        return (byte)this.mStream.ReadByte();
    }

    public void Read(ref short value)
    {
        value = this.mReader.ReadInt16();       
    }

    public short ReadShort()
    {
        return this.mReader.ReadInt16();
    }

    public void Read(ref ushort value)
    {
        value = this.mReader.ReadUInt16();
    }

    public ushort ReadUShort()
    {
        return this.mReader.ReadUInt16();
    }

    public void Read(ref int value)
    {
        value = this.mReader.ReadInt32();        
    }

    public int ReadInt()
    {
        return this.mReader.ReadInt32();
    }

    public void Read(ref uint value)
    {
        value = this.mReader.ReadUInt32();        
    }

    public uint ReadUInt()
    {
        return this.mReader.ReadUInt32();
    }

    public void Read(ref long value)
    {
        value = this.mReader.ReadInt64();        
    }

    public long ReadLong()
    {
        return this.mReader.ReadInt64();
    }

    public void Read(ref ulong value)
    {
        value = this.mReader.ReadUInt64();        
    }

    public ulong ReadULong()
    {
        return this.mReader.ReadUInt64();
    }

    public void Read(ref float value)
    {
        value = this.mReader.ReadSingle();        
    }

    public float ReadFloat()
    {
        return this.mReader.ReadSingle();
    }

    public void Read(ref double value)
    {
        value = this.mReader.ReadDouble();       
    }

    public double ReadDouble()
    {
        return this.mReader.ReadDouble();
    }

    public byte[] ReadBytes(int size)
    {
        byte[] data = new byte[size];
        long leftSize = Stream.Length - Stream.Position;
        if (leftSize < size)
        {
            string str = string.Format("BufferReader.ReadBytes：{0}数据越界", m_BuffName);
            UnityEngine.Debug.LogError(str);
        }
        this.mReader.Read(data, 0, size);

        return data;
    } 

    public string ReadUnicodeString(int charCount)
    {
        byte[] buffer = ReadBytes(charCount * 2);
        string str = System.Text.Encoding.Unicode.GetString(buffer);

        int endIndex = str.IndexOf("\0");
        if (endIndex >= 0 && endIndex < charCount)
        {
            return str.Substring(0, endIndex);
        }
        else
        {
            return str;
        }
    }

    /*
    public string ReadUTF8String(int charCount)
    {
        string str = ReadUnicodeString(charCount);
        byte[] data = System.Text.Encoding.Unicode.GetBytes(str);
        return System.Text.Encoding.UTF8.GetString(data);
    }
    */

    public string ReadString(int charCount)
    {
        return ReadUnicodeString(charCount);
    }

    public void Read(ref bool value)
    {
        value = this.mReader.ReadBoolean();
    }

    public bool ReadBool()
    {
         return this.mReader.ReadBoolean();
    }

    public static BufferReader Create(byte[] buffer)
    {
        return new BufferReader(buffer);
    }
}
