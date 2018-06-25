using System;
public class Singleton<T> where T : new()
{
	protected static T instance;

	public static T Instance
	{
		get
		{
			return Singleton<T>.instance;
		}
	}

	protected Singleton()
	{
	}

	static Singleton()
	{
		Singleton<T>.instance = ((default(T) != null) ? default(T) : Activator.CreateInstance<T>());
	}
}
