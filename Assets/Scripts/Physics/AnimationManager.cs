using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
	[SerializeField] private Material sandMat;
    public Animator[] animators;
	public static Dictionary<int, Animator> _animators = new Dictionary<int, Animator>();

    private void Awake()
    {
		foreach (Animator animator in animators)
			_animators.Add(_animators.Count, animator);
	}

    public static IEnumerator StartAnimations()
	{
		yield return new WaitForSeconds(1f);
		foreach (KeyValuePair<int, Animator> entry in _animators)
		{
			Debug.Log("play");
			entry.Value.enabled = true;
			entry.Value.Play("Base Layer.Animation", -1, 0f);
		}
	}

}
