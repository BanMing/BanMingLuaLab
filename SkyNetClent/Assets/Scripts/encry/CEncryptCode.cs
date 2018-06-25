using System;

internal class CEncryptCode
{
	public byte[] m_bufEncrypt1 = new byte[256];
	public byte[] m_bufEncrypt2 = new byte[256];

	public CEncryptCode(byte a1, byte b1, byte c1, byte fst1, byte a2, byte b2, byte c2, byte fst2)
	{
		try
		{
			byte b3 = fst1;
			for (int i = 0; i < 256; i++)
			{
				this.m_bufEncrypt1[i] = b3;
				int num = (int)(a1 * b3) % 256;
				b3 = Convert.ToByte(((int)c1 + num * (int)b3 + (int)(b1 * b3)) % 256);
			}
			b3 = fst2;
			for (int i = 0; i < 256; i++)
			{
				this.m_bufEncrypt2[i] = b3;
				int num2 = (int)(a2 * b3);
				b3 = Convert.ToByte(((int)b2 + num2) * (int)b3 + (int)c2 & 255);
			}
		}
		catch
		{
		}
	}
}
