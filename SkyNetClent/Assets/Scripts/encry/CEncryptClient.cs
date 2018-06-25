using System;

internal class CEncryptClient
{
	private static CEncryptClient m_instance;
	protected int m_nSPos1;
	protected int m_nSPos2;
	protected int m_nRPos1;
	protected int m_nRPos2;
	protected CEncryptCode m_cGlobalSendEncrypt;
	protected CEncryptCode m_cGlobalRevEncrypt;
	public static CEncryptClient instance
	{
		get
		{
			if (CEncryptClient.m_instance == null)
			{
				CEncryptClient.m_instance = new CEncryptClient();
			}
			return CEncryptClient.m_instance;
		}
	}
	public CEncryptClient()
	{
		this.Init();
	}
	internal void Init()
	{
		this.m_nSPos1 = 0;
		this.m_nSPos2 = 0;
		this.m_nRPos1 = 0;
		this.m_nRPos2 = 0;
		this.m_cGlobalSendEncrypt = new CEncryptCode(106, 205, 55, 154, 101, 47, 48, 126);
		this.m_cGlobalRevEncrypt = new CEncryptCode(106, 205, 55, 154, 101, 47, 48, 126);
	}
    public void Reset()
    {
        this.m_nSPos1 = 0;
        this.m_nSPos2 = 0;
        this.m_nRPos1 = 0;
        this.m_nRPos2 = 0;
    }
	public byte[] SendEncrypt(byte[] bufMsg, int length, bool bMove = true)
	{
		try
		{
			int nSPos = this.m_nSPos1;
			int nSPos2 = this.m_nSPos2;
			for (int i = 0; i < length; i++)
			{
				int expr_1C_cp_1 = i;
				bufMsg[expr_1C_cp_1] ^= this.m_cGlobalSendEncrypt.m_bufEncrypt1[this.m_nSPos1];
				int expr_42_cp_1 = i;
				bufMsg[expr_42_cp_1] ^= this.m_cGlobalSendEncrypt.m_bufEncrypt2[this.m_nSPos2];
				if (++this.m_nSPos1 >= 256)
				{
					this.m_nSPos1 = 0;
					if (++this.m_nSPos2 >= 256)
					{
						this.m_nSPos2 = 0;
					}
				}
			}
			if (!bMove)
			{
				this.m_nSPos1 = nSPos;
				this.m_nSPos2 = nSPos2;
			}
		}
		catch
		{
		}
		return bufMsg;
	}

	public byte[] RecEncrypt(byte[] bufMsg, int length, bool bMove = true)
	{
		try
		{
			int nRPos = this.m_nRPos1;
			int nRPos2 = this.m_nRPos2;
			for (int i = 0; i < length; i++)
			{
				int expr_1C_cp_1 = i;
				bufMsg[expr_1C_cp_1] ^= this.m_cGlobalRevEncrypt.m_bufEncrypt1[this.m_nRPos1];
				int expr_42_cp_1 = i;
				bufMsg[expr_42_cp_1] ^= this.m_cGlobalRevEncrypt.m_bufEncrypt2[this.m_nRPos2];
				if (++this.m_nRPos1 >= 256)
				{
					this.m_nRPos1 = 0;
					if (++this.m_nRPos2 >= 256)
					{
						this.m_nRPos2 = 0;
					}
				}
			}
			if (!bMove)
			{
				this.m_nRPos1 = nRPos;
				this.m_nRPos2 = nRPos2;
			}
		}
		catch
		{
		}
		return bufMsg;
	}

	public void ChangeCode(uint dwData)
	{
		try
		{
			uint value = dwData * dwData;
			byte[] bytes = BitConverter.GetBytes(dwData);
			byte[] bytes2 = BitConverter.GetBytes(value);
			for (int i = 0; i < 256; i += 4)
			{
				byte[] expr_2A_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt1;
				int expr_2A_cp_1 = i;
				expr_2A_cp_0[expr_2A_cp_1] ^= bytes[0];
				byte[] expr_4D_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt1;
				int expr_4D_cp_1 = i + 1;
				expr_4D_cp_0[expr_4D_cp_1] ^= bytes[1];
				byte[] expr_70_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt1;
				int expr_70_cp_1 = i + 2;
				expr_70_cp_0[expr_70_cp_1] ^= bytes[2];
				byte[] expr_93_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt1;
				int expr_93_cp_1 = i + 3;
				expr_93_cp_0[expr_93_cp_1] ^= bytes[3];
				byte[] expr_B4_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt2;
				int expr_B4_cp_1 = i;
				expr_B4_cp_0[expr_B4_cp_1] ^= bytes2[0];
				byte[] expr_D7_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt2;
				int expr_D7_cp_1 = i + 1;
				expr_D7_cp_0[expr_D7_cp_1] ^= bytes2[1];
				byte[] expr_FA_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt2;
				int expr_FA_cp_1 = i + 2;
				expr_FA_cp_0[expr_FA_cp_1] ^= bytes2[2];
				byte[] expr_11D_cp_0 = this.m_cGlobalSendEncrypt.m_bufEncrypt2;
				int expr_11D_cp_1 = i + 3;
				expr_11D_cp_0[expr_11D_cp_1] ^= bytes2[3];
			}
		}
		catch
		{
		}
	}
}
