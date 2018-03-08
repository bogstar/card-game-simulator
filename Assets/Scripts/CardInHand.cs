using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInHand : MonoBehaviour
{
	public int orderInHand;
	public PlayerHand hand;
	public Card card;

	public bool hoverable;
	public bool active;

	float targetAngle;
	float targetScale;
	Vector3 targetTranslation;

	Vector3 currentVelocity;
	float currentVelocityAngle;
	Vector3 currentVelocityScale;

	float smoothTime = .1f;

	public void StartChanging(Vector3 targetT, float targetA, float targetS)
	{
		targetTranslation = targetT;
		targetAngle = targetA;
		targetScale = targetS;
	}

	void Update()
	{
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetTranslation, ref currentVelocity, smoothTime);
		transform.eulerAngles = new Vector3(0, 0, Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref currentVelocityAngle, smoothTime));
		transform.localScale = Vector3.SmoothDamp(transform.localScale, Vector3.one * targetScale, ref currentVelocityScale, smoothTime);
	}
}