

using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MemoryOutputStream
{
	private byte[] _buffer;
	private int _bufferSize;
	private int _pos;

	public MemoryOutputStream(int bufferSize)
	{
		this._bufferSize = bufferSize;
		this._buffer = new byte[this._bufferSize];
	}

	public MemoryOutputStream(byte[] buffer, int bufferSize)
	{
		this._buffer = buffer;
		this._bufferSize = bufferSize;
	}

	public void Push<T>(T s) where T : new()
	{
		try
		{
			int num = Marshal.SizeOf(typeof(T));
			if (this._pos + num > this._bufferSize)
			{
				Debug.LogError("MemoryOutputStream.Push: Reach Buffer End");
				this._pos = this._bufferSize;
			}
			else
			{
				IntPtr intPtr = Marshal.AllocHGlobal(num);
				Marshal.StructureToPtr(s, intPtr, true);
				Marshal.Copy(intPtr, this._buffer, this._pos, num);
				Marshal.FreeHGlobal(intPtr);
				this._pos += num;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("MemoryOutputStream.Push:" + ex.Message);
		}
	}

	public void PushByteArray(byte[] data)
	{
		if (this._pos + data.Length > this._bufferSize)
		{
			Debug.LogError("MemoryOutputStream.PushByteArray: Reach Buffer End");
			this._pos = this._bufferSize;
			return;
		}
		Array.Copy(data, 0, this._buffer, this._pos, data.Length);
		this._pos += data.Length;
	}

	public byte[] GetBuffer()
	{
		return this._buffer;
	}

	public int GetUsedBufferSize()
	{
		return this._pos;
	}

	public void Clear()
	{
		this._pos = 0;
	}
}
