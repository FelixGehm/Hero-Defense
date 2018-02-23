using UnityEngine;
using System.Diagnostics;

public class DebugWidget : MonoBehaviour
{
	public static GameObject CreateDebugCross(Transform par = null)
	{
#if UNITY_EDITOR

		GameObject go = new GameObject();
		CreateAxisLine(new Vector3(5f, .1f, .1f), Color.red, go.transform);
		CreateAxisLine(new Vector3(.1f, 5f, .1f), Color.green, go.transform);
		CreateAxisLine(new Vector3(.1f, .1f, 5f), Color.blue, go.transform);

		go.name = "DebugCross";
		go.transform.parent = par;
		go.transform.localPosition = new Vector3(0, 0, 0);
		go.transform.localRotation = new Quaternion(0, 0, 0, 1);

		return go;
#else
		return null;
#endif
	}

	private static GameObject CreateAxisLine(Vector3 size, Color color, Transform par = null)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.transform.localScale = size;

		go.GetComponent<MeshRenderer>().material.color = color;
		Destroy(go.GetComponent<Collider>());

		go.transform.parent = par;
		go.transform.localPosition = new Vector3(0, 0, 0);
		go.transform.localRotation = new Quaternion(0, 0, 0, 1);
		return go;
	}

	[Conditional("UNITY_EDITOR")]
	public static void Move(GameObject go, Vector3 pos, Quaternion rot, emotitron.Network.NST.DebugXform istype, emotitron.Network.NST.DebugXform iftype)
	{
		if (istype != iftype)
			return;

		go.transform.position = pos;
		go.transform.rotation = rot;
	}

	[Conditional("UNITY_EDITOR")]
	public static void Move(GameObject go, Vector3 pos, emotitron.Network.NST.DebugXform istype, emotitron.Network.NST.DebugXform iftype)
	{
		if (istype != iftype)
			return;

		go.transform.position = pos;
	}

	[Conditional("UNITY_EDITOR")]
	public static void Move(GameObject go, Quaternion rot, emotitron.Network.NST.DebugXform istype, emotitron.Network.NST.DebugXform iftype)
	{
		if (istype != iftype)
			return;

		go.transform.rotation = rot;
	}
}
