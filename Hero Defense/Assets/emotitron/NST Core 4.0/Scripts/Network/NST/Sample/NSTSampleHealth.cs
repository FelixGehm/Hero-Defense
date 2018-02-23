//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.Network.Compression;
using emotitron.Network.NST.HealthSystem;
using emotitron.Utilities.BitUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST.Sample
{
	[AddComponentMenu("NST/Sample Code/NST Sample Health")]

	[DisallowMultipleComponent]
	/// <summary>
	/// Example code of how to piggyback player information, such as health on the back of the regular updates.
	/// EXTRA CAUTION should be taken when reading and writing to the bitstreams as done here. While these are the most efficient way
	/// to pack data, they offer NO protection against the developer not matching up their reads and writes.
	/// If you write 5 bits to the stream, you MUST read 5 bits from the stream on the receiving end or else all following
	/// reads will be reading from the wrong place and will return corrupt values.
	/// It can be very difficult to find bugs like this as well, as the corruption will show up as errors in other components that
	/// read from the stream after this.
	/// </summary>
	public class NSTSampleHealth : NSTComponent, IVitals, INstBitstreamInjectFirst, INstOnStartServer, INstOnStartLocalPlayer, INstState
	{
		public static NSTSampleHealth lclPlyVitals;

		// Callbacks for IMonitorVitals interface that wants notifications about changes to the local player vitals
		public static List<IMonitorVitals> lclPlayerMonitors = new List<IMonitorVitals>();

		private List<IMonitorVitals> iMonitorHealth;

		[Range(0, 15)]
		[Tooltip("Sends every X update of the NST.")]
		public int updateRate = 5;

		[HideInInspector]
		public List<Vital> vitals;
		public List<Vital> Vitals
		{
			get { return vitals; }
		}

		[HideInInspector]
		public List<float> hitGroupModifers = new List<float>();

		void Reset()
		{
			// Feel free to change these starting values
			vitals = new List<Vital>(3)
			{
				new Vital(100, 50, 1f, 5f, 1f, "Health", 7),
				new Vital(100, 50, .667f, 0, 0, "Armor", 7),
				new Vital(250, 50, 1f, 1f, 15f, "Shield", 8)

			};
		}

		private int frameOffset = 2;

		public bool UpdateDue(int frameId)
		{
			return
				((frameId + frameOffset) % updateRate == 0) &&
				frameId != 0;
		}

		// Indexer that returns the spedified health stat
		public Vital this [int vitalType]
		{
			get { return vitals[(int)vitalType]; }
		}

		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();
			// Collect all interface callbacks
			iMonitorHealth = new List<IMonitorVitals>();
			nst.GetComponentsInChildren(true, iMonitorHealth); //<IMonitorHealth>(true);

			int hitGroupCount = NSTHitGroupsSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME).hitGroupTags.Count;

			// Make sure the size of the hitgroup list is correct, could be more elegant but will do for now.
			while (hitGroupModifers.Count > hitGroupCount)
				hitGroupModifers.RemoveAt(hitGroupModifers.Count - 1);
			while (hitGroupModifers.Count < hitGroupCount)
				hitGroupModifers.Add(1);
		}

		public void OnNstStartServer()
		{
			if (MasterNetAdapter.ServerIsActive && nst.State.IsAlive())
				ResetStats();
		}

		public void OnNstStartLocalPlayer()
		{
			lclPlyVitals = this;
		}

		public void Update()
		{
			if (!MasterNetAdapter.ServerIsActive)
				return;

			float timeSinceLastDamage = Time.time - lastDamageTakenTime;

			// if this networked object is alive, test for vitals regeneration.
			if (nst.State == State.Alive)
				for (int i = 0; i < vitals.Count; i++)
					if (vitals[i].regenRate != 0)
						if (timeSinceLastDamage > vitals[i].regenDelay)
							AddToVital(vitals[i].regenRate * Time.deltaTime, i);
		}
		/// <summary>
		/// Clients receive reports about health as part of their incoming streams. The server will have added them to its outgoing/mirror streams.
		/// </summary>
		/// <param name="waitingForTeleportConfirm"></param>
		public void NSTBitstreamIncomingFirst(Frame frame, Frame currFrame, ref UdpBitStream bitstream, bool isServer, bool waitingForTeleportConfirm)
		{
			// if this isn't a client, it should not be receiving this.
			if (MasterNetAdapter.ServerIsActive && !na.HasAuthority)
				return;

			if (UpdateDue(frame.frameid))
			{
				for (int i = 0; i < vitals.Count; i++)
				{
					int incval = bitstream.ReadInt(vitals[i].bitsForStat);

					// Update the value if this isn't the server
					if (!MasterNetAdapter.ServerIsActive)
						vitals[i].Value = incval;
				}
			}

			UpdateMonitors();

		}

		/// <summary>
		/// Server adds health info to altered outgoing streams that have come in from players.
		/// </summary>
		public void NSTBitstreamMirrorFirst(Frame frame, ref UdpBitStream outstream, bool isServer, bool waitingForTeleportConfirm)
		{
			if (na.HasAuthority)
				return;

			if (UpdateDue(frame.frameid))
			{
				for (int i = 0; i < vitals.Count; i++)
					outstream.WriteInt((int)vitals[i].Value, vitals[i].bitsForStat);
			}
		}

		/// <summary>
		/// Server owned objects report their health to clients here.
		/// </summary>
		public void NSTBitstreamOutgoingFirst(Frame frame, ref UdpBitStream bitstream)
		{
			if (!MasterNetAdapter.ServerIsActive)
				return;

			if (UpdateDue(frame.frameid))
			{
				for (int i = 0; i < vitals.Count; i++)
					bitstream.WriteInt((int)vitals[i].Value, vitals[i].bitsForStat);
			}
		}

		private float ModifyDamageForHitGroup(float dmg, int hitGroupMask)
		{
			// start with the default (usually should be 1f)
			float modifer = 0;

			// get the modifer with the greatest value of all of the flagged hitgroups
			for (int i = 0; i < hitGroupModifers.Count; i++)
				if ((hitGroupMask & (1 << i)) != 0)
					modifer = (Mathf.Max(modifer, hitGroupModifers[i]));

			return modifer * dmg;
		}

		float lastDamageTakenTime;

		public void ApplyDamage(float dmg, int hitGroupMask = 0)
		{
			if (!na.IsServer)
				return;

			if (dmg == 0)
				return;

			float modifiedDmg = ModifyDamageForHitGroup(dmg, hitGroupMask);

			DebugX.Log(!DebugX.logInfo ? "" :
				(Time.time + " Apply Dmg: " + dmg + " Modified Dmg: " + modifiedDmg + " hitgroup mask:" + BitTools.PrintBitMask((uint)hitGroupMask)));

			lastDamageTakenTime = Time.time;

			// Subtract damage. Start with highest index and pass mitigated damage down
			for (int i = vitals.Count - 1; i >= 0; i--)
			{
				float mitigatedDmg = modifiedDmg * vitals[i].absorbtion;
	
				// mitigated damage exceeds the entirety of this vital - take all of it.
				if (mitigatedDmg > vitals[i].Value)
				{
					modifiedDmg -= vitals[i].Value;
					vitals[i].Value = 0;
				}
				else
				{
					modifiedDmg -= mitigatedDmg;
					vitals[i].Value -= mitigatedDmg;

					// no more damage to recurse to next lower vital - we are done
					if (modifiedDmg == 0)
						break;
				}
			}

			// Kill NST if health drops to zero
			if (vitals[0].Value <= 0)
				nst.State = State.Dead;

			UpdateMonitors();

			//ReportHealth(dmg, hitGroupMask);
		}

		private void ReportHealth(float dmg, int hitGroupMask)
		{
			string str = name + " takes " +  "Dmg Hit Groups Mask " + hitGroupMask;

			for (int i = 0; i < vitals.Count; i++)
				str += vitals[i].name + ":" + vitals[i].Value + "  ";

			Debug.Log(str);

		}
		public void ResetStats()
		{
			for (int i = 0; i < vitals.Count; i++)
				vitals[i].Value = vitals[i].startValue;

			UpdateMonitors();
		}

		public void SetVital(float value, int vitalIndex)
		{
			if (!na.IsServer)
				return;

			vitals[vitalIndex].Value = value;

			UpdateMonitors();
		}

		public void AddToVital(float value, int vitalIndex)
		{
			if (!na.IsServer)
				return;

			vitals[vitalIndex].Value += value;

			UpdateMonitors();
		}

		public void UpdateMonitors()
		{
			foreach (IMonitorVitals cb in iMonitorHealth)
				cb.OnVitalsChange(this);

			if (na.IsLocalPlayer)
			{
				foreach (IMonitorVitals cb in lclPlayerMonitors)
					cb.OnVitalsChange(this);
			}
		}

		public void OnNstState(State state)
		{
			if (state.IsAlive())
				ResetStats();
		}

		public void AddCallback(IMonitorVitals iMonitorVitals)
		{
			if (!lclPlayerMonitors.Contains(iMonitorVitals))
				lclPlayerMonitors.Add(iMonitorVitals);
		}

		public void RemoveCallback(IMonitorVitals iMonitorVitals)
		{
			lclPlayerMonitors.Remove(iMonitorVitals);
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTSampleHealth))]
	[CanEditMultipleObjects]
	public class NSTSampleHealthEditor : NSTSampleHeader
	{
		public override void OnInspectorGUI()
		{

			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUILayout.Space();

			var _t = (NSTSampleHealth)target;
			NSTHitGroupsSettings hgSettings = NSTHitGroupsSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);

			var vitals = serializedObject.FindProperty("vitals");

			for (int i = 0; i < vitals.arraySize; i++)
				EditorGUILayout.PropertyField(vitals.GetArrayElementAtIndex(i));


			int count = hgSettings.hitGroupTags.Count;

			// Resize the array if it is invalid
			while (_t.hitGroupModifers.Count > count)
				_t.hitGroupModifers.RemoveAt(_t.hitGroupModifers.Count - 1);

			while (_t.hitGroupModifers.Count < count)
				_t.hitGroupModifers.Add(1f);

			EditorGUILayout.LabelField("Hit Box Group Modifiers", (GUIStyle)"BoldLabel");
			EditorGUILayout.BeginVertical("HelpBox");

			Rect r = EditorGUILayout.GetControlRect();
			EditorGUI.LabelField(r, "Hit Box Group", (GUIStyle)"BoldLabel");
			r.xMin += EditorGUIUtility.labelWidth;
			EditorGUI.LabelField(r, "Dmg Multiplier", (GUIStyle)"BoldLabel");

			for (int i = 0; i < count; i++)
			{
				_t.hitGroupModifers[i] = EditorGUILayout.FloatField(hgSettings.hitGroupTags[i], _t.hitGroupModifers[i]);
			}

			EditorGUILayout.Space();

			NSTHitGroupsSettingsEditor.DrawLinkToSettings();

			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
		}
	}

#endif
}