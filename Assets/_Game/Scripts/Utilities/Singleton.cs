using UnityEngine;
using System;

/// <summary>
/// Singleton base class
/// </summary>
public class Singleton<T> where T : class, new()
{
	private static readonly T singleton = new T();

	public static T Instance
	{
		get
		{
			return singleton;
		}
	}
}

/// <summary>
/// Singleton for mono behavior object
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T singleton;

	public static bool IsAlive
	{
		get
		{
			return singleton != null;
		}
	}

	void Reset()
	{
		gameObject.name = typeof(T).Name;
	}

	public static T Instance
	{
		get
		{
			if (SingletonMono<T>.singleton == null)
			{
				SingletonMono<T>.singleton = (T)FindObjectOfType(typeof(T));
				if (SingletonMono<T>.singleton == null)
				{
					GameObject obj = new GameObject();
					obj.name = "[@" + typeof(T).Name + "]";
					SingletonMono<T>.singleton = obj.AddComponent<T>();
				}
			}

			return SingletonMono<T>.singleton;
		}
	}

}

/// <summary>
/// Singleton for mono behavior object
/// </summary>
/// <typeparam name="T"></typeparam>
public class ManualSingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance { get; private set; }

	void Reset()
	{
		gameObject.name = typeof(T).Name;
	}

	protected virtual void Awake()
	{
		if (Instance == null)
			Instance = (T)(MonoBehaviour)this;
	}

	protected void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}
}