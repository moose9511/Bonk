using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : Component
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				// Find existing instance in the scene
				T[] objs = FindObjectsByType<T>(FindObjectsSortMode.None);

				// If no instance exists, create a new one
				if (objs.Length > 0)
				{
					T instance = objs[0];
					_instance = instance;
				}
				else
				{
					GameObject go = new GameObject();
					go.name = typeof(T).Name;
					_instance = go.AddComponent<T>();
					DontDestroyOnLoad(go);
				}

			}
			return _instance;
		}
	}

}