using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticlesEffect : MonoBehaviour
{
	public Animator animator;
	public Text amount;

	public void Init(int amount)
	{
		this.amount.text = amount.ToString();
	}

	private void Update()
	{
		if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
		{
			GameObject.Destroy(gameObject);
		}
	}
}