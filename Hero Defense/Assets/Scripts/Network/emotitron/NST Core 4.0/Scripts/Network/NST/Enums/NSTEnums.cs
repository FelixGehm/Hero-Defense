//Copyright 2018, Davin Carten, All rights reserved

using emotitron.Utilities.BitUtilities;

namespace emotitron.Network.NST
{
	// two bit state... [Active][Visibile]
	public enum State { Dead, Frozen, Invisible, Alive }

	public static class NSTSTateExtensions
	{
		public static bool IsAlive(this State s) { return s == State.Alive; }
		public static bool IsActive(this State s) { return ((int)s & 2) != 0; }
		public static bool IsVisible(this State s) { return ((int)s & 1) != 0; }
		public static State SetActive(this State s, bool b) { return (State)BitTools.SetBitInInt((int)s, 1, b); }
		public static State SetVisible(this State s, bool b) { return (State)BitTools.SetBitInInt((int)s, 0, b); }
	}

	public enum ApplyTiming { OnReceiveUpdate, OnStartInterpolate, OnEndInterpolate }
	public enum DebugXform { None, LocalSend, RawReceive, Uninterpolated, Snapshot }

	public enum UpdateType { Regular = 0, RewindCast = 1, Cust_Msg = 2, Teleport = 4 }

	public static class UpdateTypeExtensions
	{
		public static bool IsCustom(this UpdateType m)
		{
			return ((m & UpdateType.Cust_Msg) != 0);
		}

		public static bool IsRewindCast(this UpdateType m)
		{
			return ((m & UpdateType.RewindCast) != 0);
		}

		public static bool IsTeleport(this UpdateType m)
		{
			return ((m & UpdateType.Teleport) != 0);
		}
	}

	public enum RootSendType { None, LowerHalf, LowerThirds, Full }

	public static class RootSendTypeExtensions
	{
		public static bool IsPosType(this RootSendType m)
		{
			return m != RootSendType.None;
		}

		public static bool IsFull(this RootSendType m)
		{
			return m == RootSendType.Full;
		}

		public static bool IsLBits(this RootSendType m)
		{
			return m == RootSendType.LowerHalf || m == RootSendType.LowerThirds;
		}

		public static bool None(this RootSendType m)
		{
			return m == RootSendType.None;
		}
	}



}
