//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Network.Compression;

namespace emotitron.Network.NST
{
	/// <summary>
	/// Compress vector3 to the scale of the map.
	/// </summary>
	public static class WorldVectorCompression
	{
		// constructor
		static WorldVectorCompression()
		{
			Bounds bounds = NSTMapBounds.CombinedWorldBounds;

			axisRanges = new FloatRange[3];

			for (int axis = 0; axis < 3; axis++)
				axisRanges[axis] = new FloatRange(axis, bounds.min[axis], bounds.max[axis], NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME).minPosResolution);
		}

		public static FloatRange[] axisRanges = new FloatRange[3];

		/// <summary>
		/// Change the axisranges for the world bounds to a new bounds.
		/// </summary>
		public static void EstablishMinBitsPerAxis(Bounds bounds, bool silent = false)
		{

			NSTSettings nstSettings = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);

			DebugX.LogWarning(!DebugX.logWarnings ? "" : 
				("<b>Scene is missing map bounds</b>, defaulting to a map size of Center:" + NSTMapBounds.CombinedWorldBounds.center + " Size:" + NSTMapBounds.CombinedWorldBounds.size +
				". Be sure to add NSTMapBounds components to your scene to define its bounds, or be sure the default bounds in NSTSettings are what you want."),
				(!silent && Application.isPlaying && (!NSTMapBounds.isInitialized || NSTMapBounds.ActiveBoundsCount == 0) && Time.time > 1)
				);

			DebugX.LogWarning(!DebugX.logWarnings ? "" :
				("<b>Scene map bounds are very small</b>. Current world bounds are " + bounds.center + " Size:" + bounds.size + ", is this intentional?" +
				"If not check that your NSTMapBounds fully encompass your world as intended, or if using the Default bounds set in NSTSettings, that it is correct."),
				(!silent && Application.isPlaying && (!NSTMapBounds.isInitialized || NSTMapBounds.ActiveBoundsCount > 0) && (bounds.size.x <= 1 || bounds.size.y <= 1 || bounds.size.z <= 1))
				);

			for (int axis = 0; axis < 3; axis++)
			{
				axisRanges[axis].ChangeRange(bounds.min[axis], bounds.max[axis], (nstSettings != null) ? nstSettings.minPosResolution : 100);
			}
			DebugX.Log( 
				("Notice: Change in Map Bounds (Due to an NSTBounds being added or removed from the scene) to \n" +
				"Center:"+ bounds.center +" Size:" + bounds.size + ". Be sure this map change is happening to all networked clients or things will break badly. \n" +
				"Position keyframes will use x:" + axisRanges[0].bits + " bits, y:" + axisRanges[1].bits + "bits, and z:" + axisRanges[2].bits + 
				" bits at the current minimum resolutons settings (in NST Settings)."), !silent && Application.isPlaying, true);
		}

		public static void WriteWorldCompPosToBitstream(this CompressedElement compressedpos, ref UdpBitStream bitstream, IncludedAxes ia, bool lowerBitsOnly = false)
		{
			for (int axis = 0; axis < 3; axis++)
			{
				if (ia.IsXYZ(axis)) bitstream.WriteUInt(compressedpos[axis], lowerBitsOnly ? axisRanges[axis].lowerBits : axisRanges[axis].bits);
			}
		}

		public static void WriteCompressedAxisToBitstream(this uint val, int axis, ref UdpBitStream bitstream, bool lowerBitsOnly = false)
		{
			bitstream.WriteUInt(val, lowerBitsOnly ? axisRanges[axis].lowerBits : axisRanges[axis].bits);
		}

		public static uint WriteAxisToBitstream(this float val, int axis, ref UdpBitStream bitstream, bool lowerBitsOnly = false)
		{
			uint compressedAxis = val.CompressAxis(axis);
			bitstream.WriteUInt(val.CompressAxis(axis), lowerBitsOnly ? axisRanges[axis].lowerBits : axisRanges[axis].bits);
			return compressedAxis;
		}

		public static uint CompressAxis(this float val, int axis)
		{
			return axisRanges[axis].Encode(val);
		}

		public static CompressedElement CompressToWorld(this Vector3 pos)
		{
			return new CompressedElement(
				axisRanges[0].Encode(pos.x),
				axisRanges[1].Encode(pos.y),
				axisRanges[2].Encode(pos.z));

		}

		public static int ReadCompressedAxisFromBitstream(ref UdpBitStream bitstream, int axis, bool lowerBitsOnly = false)
		{
			return (bitstream.ReadInt(lowerBitsOnly ? axisRanges[axis].lowerBits : axisRanges[axis].bits));
		}

		public static float ReadAxisFromBitstream(ref UdpBitStream bitstream, int axis, bool lowerBitsOnly = false)
		{
			uint compressedAxis = bitstream.ReadUInt(lowerBitsOnly ? axisRanges[axis].lowerBits : axisRanges[axis].bits);

			return compressedAxis.DecompressAxis(axis);
		}

		private static float DecompressAxis(this uint val, int axis)
		{
			return axisRanges[axis].Decode(val);
		}

		public static CompressedElement ReadCompressedPosFromBitstream(ref UdpBitStream bitstream, IncludedAxes ia, bool lowerBitsOnly = false)
		{
			return new CompressedElement(
				(ia.IsXYZ(0)) ? (bitstream.ReadUInt(lowerBitsOnly ? axisRanges[0].lowerBits : axisRanges[0].bits)) : 0,
				(ia.IsXYZ(1)) ? (bitstream.ReadUInt(lowerBitsOnly ? axisRanges[1].lowerBits : axisRanges[1].bits)) : 0,
				(ia.IsXYZ(2)) ? (bitstream.ReadUInt(lowerBitsOnly ? axisRanges[2].lowerBits : axisRanges[2].bits)) : 0);
		}

		private static Vector3 Decompress(uint x, uint y, uint z)
		{
			return new Vector3
				(
					axisRanges[0].Decode(x),
					axisRanges[1].Decode(y),
					axisRanges[2].Decode(z)
				);
		}
		public static Vector3 DecompressFromWorld(this CompressedElement compos)
		{
			return new Vector3
				(
					axisRanges[0].Decode(compos.x),
					axisRanges[1].Decode(compos.y),
					axisRanges[2].Decode(compos.z)
				);
		}

		public static Vector3 ClampAxes(Vector3 value)
		{
			return new Vector3(
				axisRanges[0].Clamp(value[0]),
				axisRanges[1].Clamp(value[1]),
				axisRanges[2].Clamp(value[2])
				);
		}


		private static bool TestMatchingUpper(uint a, uint b, int lowerbits)
		{
			return (((a >> lowerbits) << lowerbits) == ((b >> lowerbits) << lowerbits));
		}

		//TODO make this accept an lowerbit level (half/twothirds)
		public static bool TestMatchingUpper(CompressedElement prevPos, CompressedElement b)
		{
			return
				(
				TestMatchingUpper(prevPos.x, b.x, axisRanges[0].lowerBits) &&
				TestMatchingUpper(prevPos.y, b.y, axisRanges[1].lowerBits) &&
				TestMatchingUpper(prevPos.z, b.z, axisRanges[2].lowerBits)
				);
		}

		/// <summary>
		/// Attempts to guess the most likely upperbits state by seeing if each axis of the new position would be
		/// closer to the old one if the upper bit is incremented by one, two, three etc. Stops trying when it fails to get a better result than the last increment.
		/// </summary>
		/// <param name="oldcpos">Last best position test against.</param>
		/// <returns>Returns a corrected CompressPos</returns>
		public static CompressedElement GuessUpperBitsWorld(this CompressedElement newcpos, CompressedElement oldcpos)
		{
			return newcpos.GuessUpperBits(oldcpos, axisRanges);
		}

		public static CompressedElement ZeroLowerBits(this CompressedElement fullpos)
		{
			return new CompressedElement(
				axisRanges[0].ZeroLowerBits(fullpos.x),
				axisRanges[1].ZeroLowerBits(fullpos.y),
				axisRanges[2].ZeroLowerBits(fullpos.z)
				);
		}

		public static CompressedElement ZeroUpperBits(this CompressedElement fullpos)
		{
			return new CompressedElement(
				axisRanges[0].ZeroUpperBits(fullpos.x),
				axisRanges[1].ZeroUpperBits(fullpos.y),
				axisRanges[2].ZeroUpperBits(fullpos.z)
				);
		}

		public static CompressedElement OverwriteLowerBits(CompressedElement upperbits, CompressedElement lowerbits)
		{
			return new CompressedElement
			(
				axisRanges[0].ZeroLowerBits(upperbits[0]) | lowerbits[0],
				axisRanges[1].ZeroLowerBits(upperbits[1]) | lowerbits[1],
				axisRanges[2].ZeroLowerBits(upperbits[2]) | lowerbits[2]
			);
		}

	}
}



