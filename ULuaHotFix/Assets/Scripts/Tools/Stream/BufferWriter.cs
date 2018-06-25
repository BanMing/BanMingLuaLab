

using System;
using System.IO;

public class BufferWriter
{
	private MemoryStream mStream;
	private BinaryWriter mWriter;

    private MemoryStream Stream
	{
		get
		{
			return this.mStream;
		}
	}

    public int UsedLength
    {
        get
        {
            if (mStream != null)
            {
                return (int)mStream.Position;
            }

            return 0;
        }
    }

	public BufferWriter(int size)
	{
		this.mStream = new MemoryStream(size);
		this.mWriter = new BinaryWriter(this.mStream);
	}

	public void Write(sbyte value)
	{
		this.mWriter.Write(value);
	}

    public void WriteSByte(sbyte value)
    {
        this.mWriter.Write(value);
    }

	public void Write(byte value)
	{
		this.mWriter.Write(value);
	}

    public void WriteByte(byte value)
    {
        this.mWriter.Write(value);
    }

    public void Write(short value)
	{
		this.mWriter.Write(value);
	}

    public void WriteShort(short value)
    {
        this.mWriter.Write(value);
    }

    public void Write(ushort value)
	{
		this.mWriter.Write(value);
	}

    public void WriteUShort(ushort value)
    {
        this.mWriter.Write(value);
    }

    public void Write(int value)
	{
		this.mWriter.Write(value);
	}

    public void WriteInt(int value)
    {
        this.mWriter.Write(value);
    }

    public void Write(uint value)
	{
		this.mWriter.Write(value);
	}

    public void WriteUInt(uint value)
    {
        this.mWriter.Write(value);
    }

    public void Write(long value)
	{
		this.mWriter.Write(value);
	}

    public void WriteLong(long value)
    {
        this.mWriter.Write(value);
    }

    public void Write(ulong value)
	{
		this.mWriter.Write(value);
	}

    public void WriteULong(ulong value)
    {
        this.mWriter.Write(value);
    }

    public void Write(float value)
	{
		this.mWriter.Write(value);
	}

    public void WriteFloat(float value)
    {
        this.mWriter.Write(value);
    }

    public void Write(double value)
	{
		this.mWriter.Write(value);
	}

    public void WriteDouble(double value)
    {
        this.mWriter.Write(value);
    }

    public void Write(byte[] value)
	{
		this.mWriter.Write(value);
	}

    public void WriteBytes(byte[] value)
    {
        this.mWriter.Write(value);
    }

    public void Write(byte[] value, int index, int count)
    {
        this.mWriter.Write(value, index, count);
    }

    public void WriteBytes(byte[] value, int index, int count)
    {
        this.mWriter.Write(value, index, count);
    }

    //д���ַ�����charCount�ַ�����
    public void WriteUnicodeString(string str, int charCount)
    {
        byte[] data = new byte[charCount * 2];
        int dataSize = Math.Min(charCount, str.Length);
        System.Text.Encoding.Unicode.GetBytes(str, 0, dataSize, data, 0);
        WriteBytes(data, 0, data.Length);
    }

    /*
    public void WriteUTF8String(string str, int charCount)
    {
        byte[] buffUTF8 = System.Text.Encoding.UTF8.GetBytes(str);
        string strUnicode = System.Text.Encoding.Unicode.GetString(buffUTF8);
        WriteUnicodeString(strUnicode, charCount);
    }
    */

    public void WriteString(string str, int charCount)
    {
        WriteUnicodeString(str, charCount);
    }

    public void Write(bool value)
    {
        this.mWriter.Write(value);
    }

    public void WriteBool(bool value)
    {
        this.mWriter.Write(value);
    }

    public void Clear()
	{
		this.mStream.Position = 0L;
		this.mStream.SetLength(0L);
	}

    public byte[] GetBuffer()
    {
        if(mStream != null)
        {
            return mStream.GetBuffer();
        }

        return null;   
    }

    public int GetUsedSize()
    {
        if(mStream != null)
        {
            return (int)mStream.Position;
        }

        return 0;
    }

    static public BufferWriter Create(int size)
    {
        return new BufferWriter(size);
    }
}

//public class GlobalBufferWriter
//{
//    static public BufferWriter writer1024 = new BufferWriter(1024);
//    static public BufferWriter writer2048 = new BufferWriter(2048);
//    static public BufferWriter writer4096 = new BufferWriter(4096);
//    static public BufferWriter writer8192 = new BufferWriter(8192);
//}