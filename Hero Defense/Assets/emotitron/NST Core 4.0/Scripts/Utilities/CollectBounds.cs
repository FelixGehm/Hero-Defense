//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Utilities
{
	public static class BoundsTools
	{
		public enum BoundsType { Both, MeshRenderer, Collider }

		private static List<MeshFilter> reusableSearchMeshFilter = new List<MeshFilter>();
		private static List<MeshRenderer> reusableSearchMeshRend = new List<MeshRenderer>();
		private static List<Collider> reusableSearchColliders = new List<Collider>();

		/// <summary>
		/// Collect the bounds of the indicated types (MeshRenderer and/or Collider) on the object and all of its children, and returns bounds that are a sum of all of those.
		/// </summary>
		/// <param name="go">GameObject to start search from.</param>
		/// <param name="factorIn">The types of bounds to factor in.</param>
		/// <param name="includeChildren">Whether to search all children for bounds.</param>
		/// <returns></returns>
		public static Bounds CollectMyBounds(GameObject go, BoundsType factorIn, out int numOfBoundsFound, bool includeChildren = true, bool includeInactive = false)
		{
			bool bothtype = factorIn == BoundsType.Both;
			bool rendtype = bothtype || factorIn == BoundsType.MeshRenderer;
			bool colltype = bothtype || factorIn == BoundsType.Collider;

			// Clear the reusables so they have counts of zero
			reusableSearchMeshFilter.Clear();
			reusableSearchMeshRend.Clear();
			reusableSearchColliders.Clear();
			int myBoundsCount = 0;

			// Find all of the MeshRenderers and Colliders (as specified)
			if (rendtype)
			{
				if (go.activeInHierarchy)
				{
					if (includeChildren)
						go.GetComponentsInChildren(includeInactive, reusableSearchMeshFilter);
					else
						go.GetComponents(reusableSearchMeshFilter);
				}
			}

			if (colltype)
			{
				if (go.activeInHierarchy)
				{
					if (includeChildren)
						go.GetComponentsInChildren(includeInactive, reusableSearchColliders);
					else
						go.GetComponents(reusableSearchColliders);
				}
			}

			// Add any MeshRenderer attached to the found MeshFilters to their own list.
			// We want the MeshRenderer for its bounts, but only if there is a MeshFilter, otherwise there is a risk of a 0,0,0
			for (int i = 0; i < reusableSearchMeshFilter.Count; i++)
			{
				MeshRenderer mr = reusableSearchMeshFilter[i].GetComponent<MeshRenderer>();

				if (mr)
					reusableSearchMeshRend.Add(mr);
			}

			// Make sure we found some bounds objects, or we need to quit.
			numOfBoundsFound = reusableSearchMeshRend.Count + reusableSearchColliders.Count;
			// No values means no bounds will be found, and this will break things if we try to use it.
			if (numOfBoundsFound == 0)
			{
				return new Bounds();
			}

			// Get a starting bounds. We need this because the default of centered 0,0,0 will break things if the map is
			// offset and doesn't encapsulte the world origin.
			Bounds compositeBounds = (reusableSearchMeshRend.Count > 0) ? reusableSearchMeshRend[0].bounds : reusableSearchColliders[0].bounds;


			// Encapsulate all outer found bounds into that. We will be adding the root to itself, but no biggy, this only runs once.
			for (int i = 0; i < reusableSearchMeshRend.Count; i++)
			{
				myBoundsCount++;
				compositeBounds.Encapsulate(reusableSearchMeshRend[i].bounds);
			}

			for (int i = 0; i < reusableSearchColliders.Count; i++)
			{
				myBoundsCount++;
				compositeBounds.Encapsulate(reusableSearchColliders[i].bounds);
			}

			return compositeBounds;

		}

		public static Bounds CollectMyBounds(GameObject go, BoundsType factorIn, bool includeChildren = true)
		{
			int dummy;
			return CollectMyBounds(go, factorIn, out dummy, includeChildren);
		}

	}
}

