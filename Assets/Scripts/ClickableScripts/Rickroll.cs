using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rickroll : MonoBehaviour, IClickable
{
	public void onClick()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=xvFZjo5PgG0&ab_channel=Duran"); // Never gonna give you uuuup
	}
}
