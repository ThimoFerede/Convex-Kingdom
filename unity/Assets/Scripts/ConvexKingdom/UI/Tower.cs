using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
	public Vector2 Pos { get; private set; }

	private Controller m_controller;

	void Awake()
	{
		Pos = new Vector2(transform.position.x, transform.position.y);
		m_controller = FindObjectOfType<Controller>();
	}

	void OnMouseUpAsButton()
	{
		// claim or battle for tower
	}
}
