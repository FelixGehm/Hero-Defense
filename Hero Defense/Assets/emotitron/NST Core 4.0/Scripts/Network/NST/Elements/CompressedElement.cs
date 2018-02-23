//Copyright 2018, Davin Carten, All rights reserved

using emotitron.Utilities.BitUtilities;
using System.Runtime.InteropServices;
using emotitron.Network.Compression;

namespace emotitron.Network.NST
{
	[StructLayout(LayoutKind.Explicit)]
	public struct CompressedElement
	{
		[FieldOffset(0)]
		public uint x;
		[FieldOffset(4)]
		public uint y;
		[FieldOffset(8)]
		public uint z;

		[FieldOffset(0)]
		public float floatx;
		[FieldOffset(4)]
		public float floaty;
		[FieldOffset(8)]
		public float floatz;

		[FieldOffset(0)]
		public ulong quat;

		public readonly static CompressedElement zero;

		public CompressedElement(uint _x, uint _y, uint _z)
		{
			this = default(CompressedElement);
			x = _x;
			y = _y;
			z = _z;
		}
		public CompressedElement(ushort _x, ushort _y, ushort _z)
		{
			this = default(CompressedElement);
			x = _x;
			y = _y;
			z = _z;
		}
		public CompressedElement(float _x, float _y, float _z)
		{
			this = default(CompressedElement);
			floatx = _x;
			floaty = _y;
			floatz = _z;
		}

		public CompressedElement(ulong _quat)
		{
			this = default(CompressedElement);
			quat = _quat;
		}

		static CompressedElement()
		{
			zero = new CompressedElement() { x = 0, y = 0, z = 0 };
		}

		// Indexer
		public uint this[int index]
		{
			get
			{
				return (index == 0) ? x : (index == 1) ? y : z;
			}
			set
			{
				if (index == 0) x = value;
				else if (index == 1) y = value;
				else if (index == 2) z = value;
			}
		}

		public float GetFloat(int axis)
		{
			return (axis == 0) ? floatx : (axis == 1) ? floaty : floatz;
		}

		public uint GetUInt(int axis)
		{
			return (axis == 0) ? x : (axis == 1) ? y : z;
		}

		public static implicit operator ulong(CompressedElement val)
		{
			return val.quat;
		}

		public static implicit operator CompressedElement(ulong val)
		{
			return new CompressedElement(val);
		}

		/// <summary>
		/// Basic compare of the X, Y, Z, and W values. True if they all match.
		/// </summary>
		public static bool Compare(CompressedElement a, CompressedElement b)
		{
			return (a.x == b.x && a.y == b.y && a.z == b.z);
		}

		public static void Copy(CompressedElement source, CompressedElement target)
		{
			target.x = source.x;
			target.y = source.y;
			target.z = source.z;
		}

		/// <summary>
		/// Get the bit count of the highest bit that is different between two compressed positions. This is the min number of bits that must be sent.
		/// </summary>
		/// <returns></returns>
		public static int HighestDifferentBit(uint a, uint b)
		{
			int highestDiffBit = 0;

			for (int i = 0; i < 32; i++)
				if (i.CompareBit(a, b) == false)
					highestDiffBit = i;

			return highestDiffBit;
		}

		public static CompressedElement operator +(CompressedElement a, CompressedElement b)
		{
			return new CompressedElement((uint)((long)a.x + b.x), (uint)((long)a.y + b.y), (uint)((long)a.z + b.z));
		}

		public static CompressedElement operator -(CompressedElement a, CompressedElement b)
		{
			return new CompressedElement((uint)((long)a.x - b.x), (uint)((long)a.y - b.y), (uint)((long)a.z - b.z));
		}
		public static CompressedElement operator *(CompressedElement a, float b)
		{
			return new CompressedElement((uint)(a.x * b), (uint)(a.y * b), (uint)(a.z * b));
		}

		public static CompressedElement Extrapolate(CompressedElement curr, CompressedElement prev, int divisor = 2)
		{
			return new CompressedElement
				(
				(uint)(curr.x + (((long)curr.x - prev.x)) / divisor),
				(uint)(curr.y + (((long)curr.y - prev.y)) / divisor),
				(uint)(curr.z + (((long)curr.z - prev.z)) / divisor)
				);
		}
		/// <summary>
		/// It is preferable to use the overload that takes and int divisor value than a float, to avoid all float math.
		/// </summary>
		public static CompressedElement Extrapolate (CompressedElement curr, CompressedElement prev, float amount = .5f)
		{
			int divisor = (int)(1f / amount);
			return Extrapolate(curr, prev, divisor);
		}

		public override string ToString()
		{
			return "[" + quat + "]" + " X:" + x + " y:" + y + " z:" + z;
		}
	}

	public static class CompressedElementExt
	{
		/// <summary>
		/// Attempts to guess the most likely upperbits state by seeing if each axis of the new position would be
		/// closer to the old one if the upper bit is incremented by one, two, three etc. Stops trying when it fails to get a better result than the last increment.
		/// </summary>
		/// <param name="oldcpos">Last best position test against.</param>
		/// <returns>Returns a corrected CompressPos</returns>
		public static CompressedElement GuessUpperBits(this CompressedElement newcpos, CompressedElement oldcpos, FloatRange[] axesranges)
		{
			CompressedElement oldUppers = oldcpos.ZeroLowerBits();
			CompressedElement newLowers = newcpos.ZeroUpperBits();
			CompressedElement bestGuess = oldUppers + newLowers;

			for (int axis = 0; axis < 3; axis++)
			{
				// value that will increase or decrease the upperbits by one
				uint increment = ((uint)1 << axesranges[axis].lowerBits);
				int multiplier = 1;

				// start by just applying the old uppers to the new lowers. This is the distance to beat.
				long lastguessdist = System.Math.Abs((long)bestGuess[axis] - oldcpos[axis]);
				bool lookup = true;
				bool lookdn = true;

				while (multiplier < 3) // 3 is arbitrary)
				{
					if (lookup)
					{
						// This will break if user is using the full 32 bits for an axis and they use lbits near a map edge. Should be longs to avoid that.
						long guessup = (long)bestGuess[axis] + increment;
						long updist = guessup - oldcpos[axis];

						// Stop looking up if : 1. this increment is worse than best so far, value is above range max
						if (updist > lastguessdist || guessup > axesranges[axis].maxvalue)
						{
							lookup = false;
						}
						else // if (updist < lastguessdist)
						{
							bestGuess[axis] = (uint)guessup;
							lastguessdist = updist;
							lookdn = false;
							continue;
						}
					}

					if (lookdn)
					{
						long guessdn = ((long)bestGuess[axis] - increment); // keep as long so we can test for < 0
						long dndist = oldcpos[axis] - guessdn;

						// Stop looking downp if : 1. this increment is worse than best so far, 2. value is below 0, or 3. value is above range max
						if (dndist > lastguessdist || guessdn < 0)
						{
							lookdn = false;
							continue; // Continue is here to prevent hitting the end break without exhausting look up
						}
						else // if (dndist < lastguessdist)
						{
							bestGuess[axis] = (uint)guessdn;
							lastguessdist = dndist;
							lookup = false;
							continue;
						}
					}

					// No improvements found, we are done looking.
					break;
				}

			}

			return bestGuess;
		}

	}
}
