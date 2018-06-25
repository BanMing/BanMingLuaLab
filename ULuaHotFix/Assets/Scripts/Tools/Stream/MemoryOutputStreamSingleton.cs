

using System;
public class MemoryOutputStreamSingleton : MemoryOutputStream
{
	private static MemoryOutputStreamSingleton _instance;
	private MemoryOutputStreamSingleton() : base(4096)
	{
	}

	public static MemoryOutputStreamSingleton GetInstance()
	{
		if (MemoryOutputStreamSingleton._instance == null)
		{
			MemoryOutputStreamSingleton._instance = new MemoryOutputStreamSingleton();
		}
		return MemoryOutputStreamSingleton._instance;
	}
}
