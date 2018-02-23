//Copyright 2018, Davin Carten, All rights reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Utilities.Pooling
{

	/// <summary>
	/// A VERY basic pooling system, because not using pooling is a bad for things like projectiles. 
	/// The statics are the pool manager. The non-static fiends and methods are for pool members this is attached to.
	/// </summary>
	public class Pool : MonoBehaviour
	{
		/// <summary>
		/// Store the creation values for when we need to grow the pool.
		/// </summary>
		private class PoolItemDef
		{
			public GameObject prefab;
			public int growBy;
			public Type scriptToAdd;
		}

		#region Static PoolManager items

		private static List<Stack<Pool>> pools = new List<Stack<Pool>>();
		private static List<PoolItemDef> poolItemDefs = new List<PoolItemDef>();

		/// <summary>
		/// Add a prefab to the Pool list, and create a pool. Returns the list index.
		/// </summary>
		/// <param name="_prefab"></param>
		/// <param name="_growBy"></param>
		/// <param name="_scriptToAdd">Indicate a typeof(Component) that you want added (if not already there) to the root of all instantiated pool items.</param>
		/// <returns></returns>
		public static int AddPrefabToPool(GameObject _prefab, int _growBy = 8, Type _scriptToAdd = null)
		{
			// if this prefab already exists as a pooled item, return the index of that pool.
			for (int i = 0; i < poolItemDefs.Count; i++)
				if (poolItemDefs[i].prefab == _prefab)
					return i;

			pools.Add(new Stack<Pool>());
			
			poolItemDefs.Add(new PoolItemDef() { prefab = _prefab, growBy = _growBy, scriptToAdd = _scriptToAdd });
			int index = pools.Count - 1;

			GrowPool(index);

			return index;
		}

		public static void GrowPool(int poolIndex)
		{
			for (int i = 0; i < poolItemDefs[poolIndex].growBy; i++)
			{
				PoolItemDef pt = poolItemDefs[poolIndex];

				GameObject go = Instantiate(pt.prefab);
				go.SetActive(false);
				Pool p =  go.AddComponent<Pool>();

				// Add the scrpitToAdd if it doesn't already exist
				if (pt.scriptToAdd != null && go.GetComponent(pt.scriptToAdd) == null)
					go.AddComponent(pt.scriptToAdd);

				p.poolIndex = poolIndex;

				pools[poolIndex].Push(p);
			}
		}

		public static void ReturnToPool(Pool p, int poolIndex)
		{
			pools[poolIndex].Push(p);
		}

		public static Pool Spawn(int poolIndex, Transform t, float duration = 5f)
		{ return Spawn(poolIndex, t.position, t.rotation, duration); }

		public static Pool Spawn(int poolIndex, Vector3 pos, Quaternion rot, float duration = 5f)
		{
			if (pools[poolIndex].Count == 0)
				GrowPool(poolIndex);

			Pool p = pools[poolIndex].Pop();
			p.transform.position = pos;
			p.transform.rotation = rot;
			p.expirationTime = (duration > 0) ? (Time.time + duration) : 0;
			p.gameObject.SetActive(true);

			// Only enable if we are counting down for expiration.
			p.enabled = (duration > 0);
			return p;
		}

		#endregion

		#region Fields and Methods used by pool items this is attached to.

		public int poolIndex;
		public float expirationTime;

		private void Update()
		{
			if (Time.time > expirationTime)
				gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			ReturnToPool(this, poolIndex);
		}

		#endregion

	}
}
