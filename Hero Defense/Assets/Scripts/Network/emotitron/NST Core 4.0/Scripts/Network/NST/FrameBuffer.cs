//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.SmartVars;
using emotitron.Utilities.BitUtilities;

namespace emotitron.Network.NST
{
	/// <summary>
	/// The Circular Buffer of Frames used by every NST. Every NST has one FrameBuffer and every Framebuffer is attached to one NST.
	/// This class contains an array of Frame[] as the buffer object.
	/// </summary>
	public class FrameBuffer
	{
		public NetworkSyncTransform nst; // the owner of this buffer
		public NSTElementsEngine nstElementsEngine;

		public Frame[] frames;

		public int bufferSize;
		public int halfBufferSize;
		public int quaterBufferSize;
		public ulong validFrameMask;

		public Frame currentFrame;
		public Frame prevAppliedFrame;

		public float bufferAverageSize;
		public int numberOfSamplesInBufferAvg;

		// Number of frames current can get behind in the buffer before jumping forward for resync. 
		//This exists because teleports clear the valid flag mask, which will trigger resyncs on larger buffers
		private int jumpForwardThreshold; 

		// Construct
		public FrameBuffer(NetworkSyncTransform _nst, NSTElementsEngine _nstElementsEngine, int _size, Vector3 _pos, Quaternion _rot)
		{
			nst = _nst;
			nstElementsEngine = _nstElementsEngine;

			bufferSize = _size;
			halfBufferSize = (bufferSize - 1) / 2;
			quaterBufferSize = bufferSize / 3;

			frames = new Frame[bufferSize];

			CompressedElement compPos = _pos.CompressToWorld();
			for (int i = 0; i < bufferSize; i++)
			{
				frames[i] = new Frame(nst, nstElementsEngine, i, _pos, compPos, _rot);
			}

			currentFrame = frames[0];
			prevAppliedFrame = frames[0];

			frames[0].ModifyFrame(UpdateType.Regular, RootSendType.Full, _pos, _rot, Time.time);

			jumpForwardThreshold = (int)(nst.desiredBufferMS / nst.frameUpdateInterval) + 1;
		}

		/// <summary>
		/// Used to create the next frame when it hasn't arrived in time over the network.
		/// </summary>
		/// <param name="svrWaitingForTeleportConfirm"></param>
		public void ExtrapolateNextFrame(bool svrWaitingForTeleportConfirm) { Extrapolate(currentFrame, prevAppliedFrame, NextFrame, svrWaitingForTeleportConfirm); }

		int extrapolationCount;

		/// <summary>
		/// Extrapolates position/rotation data from a start and end frame, and writes the results to the target frame.
		/// </summary>
		public void Extrapolate(Frame currFr, Frame prevFr, Frame targetFr, bool svrWaitingForTeleportConfirm)
		{
			extrapolationCount++;

			targetFr.state = currFr.state;

			bool isTeleport = currFr.updateType.IsTeleport();
			
			// Limit number of extrapolations, and don't extrapolate if the current frame was a teleport.
			if (svrWaitingForTeleportConfirm)
			{
				targetFr.rootPos = nst.lastSentPos; // currFr.rootPos;
				targetFr.compPos = nst.lastSentCompPos;// currFr.compPos;
			}
			else if (extrapolationCount > nst.maxExtrapolates || isTeleport)
			{
				targetFr.rootPos = currFr.rootPos;
				targetFr.compPos = currFr.compPos;
			}
			// Extrapolate root
			else
			{
				// Root Pos Extrapolation is done by the compresed positions rather than floats to avoid float error noise from turning into wiggle.
				targetFr.CompRootPos = CompressedElement.Extrapolate(currFr.compPos, prevFr.compPos, nst.extrapolationDivisor);

				//Debug.Log( 
				DebugX.Log(!DebugX.logInfo ? "" :
					(Time.time + " " + nst.name + " <color=black>Extrapolated Missing Next Frame targ:" + targetFr.frameid + " " + targetFr.rootPos + " curr:" + currFr.frameid + " " + currFr.rootPos + " prev" + prevFr.frameid + " " + prevFr.rootPos + "</color>"));
			}
			// Carry over the teleport type to generated update.
			targetFr.updateType = isTeleport ? UpdateType.Teleport : UpdateType.Regular;

			// Extrapolate elements
			foreach (INstOnExtrapolate callbacks in nst.iNstOnExtrapolate)
				callbacks.OnExtrapolate(targetFr, currFr, extrapolationCount, svrWaitingForTeleportConfirm);

		}

		public void SetBitInValidFrameMask(int bit, bool b)
		{
			bit.SetBitInMask(ref validFrameMask, b);
		}

		public bool GetBitInValidFrameMask(int bit)
		{
			return validFrameMask.GetBitInMask(bit);
		}

		/// <summary>
		/// Notifies the FrameBuffer that a frame has been populated with a new update and is now a valid frame.
		/// </summary>
		public void AddFrameToBuffer(Frame frame) // UpdateType updateType, RootSendType rootPosType, CompressedElement compPos, int frameid)
		{
			int numOfFramesFromCurrent = CountFrames(CurrentIndex, frame.frameid);
			// is this frame still a future event for interpolation, or has it already just guessed it?
			bool isStillPending = numOfFramesFromCurrent < halfBufferSize && numOfFramesFromCurrent > 0;
			bool isCurrentFrame = CurrentIndex == frame.frameid; // TODO: Redundant??

			// Set as valid if 1. is not the frame currently rendering 2. is not in the past, unless the buffer is empty then we need to rewind
			SetBitInValidFrameMask(frame.frameid, !isCurrentFrame && (isStillPending || validFrameMask == 0));
		}

		/// <summary>
		/// Determine the difference in count between two packet counts - accounting for the range being 1-X
		/// </summary>
		/// <returns> </returns>
		public int CountFrames(int firstIndex, int secondIndex)
		{
			// zero packets are reserved for indicating a teleport/fire event
			if (secondIndex == 0 || firstIndex == 0)
				return 1;

			// if the new index is lower, convert it to what it would have been had it not wrapped back around.
			if (secondIndex < firstIndex)
				secondIndex += (frames.Length - 1); // zero is skipped so we account for that here with the -1

			int numOfIndexes = secondIndex - firstIndex;

			return numOfIndexes;
		}

		public const int AVG_BUFFER_MAX_SAMPLES = 5;

		/// <summary>
		/// Factors a new buffer size (in seconds) into the running buffer average.
		/// </summary>
		/// <param name="newTime"></param>
		public void AddTimeToBufferAverage(float newTime)
		{
			bufferAverageSize = (bufferAverageSize * numberOfSamplesInBufferAvg + newTime) / (numberOfSamplesInBufferAvg + 1);
			numberOfSamplesInBufferAvg = Mathf.Min(numberOfSamplesInBufferAvg + 1, AVG_BUFFER_MAX_SAMPLES);
		}

		/// <summary>
		/// Factor the current buffer size into the running average.
		/// </summary>
		public void UpdateBufferAverage()
		{
			if (CurrentIndex == 0)
				return;

			AddTimeToBufferAverage(CurrentBufferSize);
		}

		/// <summary>
		/// Get the current size of the buffer. Accounts for number of frames in the buffer + remaining interpolation on current frame.
		/// </summary>
		public float CurrentBufferSize
		{
			get
			{
				int numOfTrueBits = validFrameMask.CountTrueBits();
				int steps;

				// Don't bother with the complex check for numb of frames in buffer if we have less than 2 bits
				if (numOfTrueBits <= 1)
				{
					steps = numOfTrueBits;
				}
				// loss tolerent check for number of frames in buffer.
				else
				{
					int oldest = 0;
					int newest = 0;

					for (int i = quaterBufferSize - 1; i > 0; i--)
					{
						if (GetBitInValidFrameMask(Increment(CurrentIndex, i)))
						{
							newest = i;
							break;
						}
					}
					for (int i = -(quaterBufferSize - 1); i <= 0; i++)
					{
						if (GetBitInValidFrameMask(Increment(CurrentIndex, i)))
						{
							oldest = i;
							break;
						}
					}
					steps = 1 + newest - oldest;
				}
				// TODO: clamp likely unneeded
				return steps * nst.frameUpdateInterval + Mathf.Clamp(currentFrame.endTime - Time.time, 0, nst.frameUpdateInterval);
			}
		}

		public int CurrentIndex
		{
			get { return currentFrame.frameid; }
		}

		/// <summary>
		/// Get the current frame index + 1
		/// </summary>
		public int GetNextIndex
		{
			get
			{
				int next = currentFrame.frameid + 1;
				if (next >= frames.Length)
					next -= frames.Length - 1;

				return next;
			}
		}

		/// <summary>
		/// Get the current frame index - 1
		/// </summary>
		public int GetPrevIndex
		{
			get
			{
				int previndex = currentFrame.frameid - 1;
				if (previndex < 1)
					previndex = frames.Length - 1;

				return previndex;
			}
		}

		public Frame NextFrame { get { return frames[GetNextIndex]; } }
		public Frame PrevFrame { get { return frames[GetPrevIndex]; } }
		public Frame IncrementFrame(int startingId, int increment) { return frames[Increment(startingId, increment)]; }

		/// <summary>
		/// Get the frame X increments before or after another frame.
		/// </summary>
		public int Increment(int startIndex, int increment)
		{
			int newIndex = startIndex + increment;

			while (newIndex >= bufferSize)
				newIndex -= (bufferSize - 1);

			while (newIndex < 1)
				newIndex += (bufferSize - 1);

			return newIndex;
		}

		/// <summary>
		/// Find frame in buffer x increments from the given frame.
		/// </summary>
		public Frame IncrementFrame(Frame startingFrame, int increment)
		{
			return IncrementFrame(startingFrame.frameid, increment);
		}

		/// <summary>
		/// Returns the previous keyframe closest to the specified frame, if none can be found returns the current frame.
		/// </summary>
		/// <param name="index"></param>
		/// <returns>Returns current frame if no keyframes are found.</returns>
		public Frame BestPreviousKeyframe(int index)
		{
			//// First try to get best keyframe
			for (int i = 1; i < halfBufferSize; i++)
			{
				int offsetIndex = index - i;
				int correctedi = (offsetIndex < 1) ? offsetIndex + frames.Length - 1 : offsetIndex;
				Frame frame = frames[correctedi];

				if (frame.rootSendType.IsFull() &&
					Time.time - frame.packetArriveTime < nst.frameUpdateInterval * (i + quaterBufferSize)) // rough estimate that the frame came in this round and isn't a full buffer cycle old
				{
					return frame;
				}
			}

			//Debug.Log(
			DebugX.LogWarning(!DebugX.logWarnings ? "" : 
				(Time.time + " NST:" + nst.NstId + " " + nst.name + " <color=black>Could not find a recent keyframe in the buffer history, likely very bad internet loss is responsible. " +
					"Some erratic player movement is possible.</color>"));

			return currentFrame;
		}

		/// <summary>
		/// This is a hot path. Runs at the completion interpolation, and attempts to find/reconstruct the next suitable frame for interpolation.
		/// </summary>
		public Frame DetermineAndPrepareNextFrame(bool svrWaitingForTeleportConfirm)
		{
			// buffer is empty, no point looking for any frames - we need to extrapolate the next frame
			if (validFrameMask <= 1)
			{
				DebugX.Log(!DebugX.logInfo ? "" :
				//Debug.Log(
					(Time.time + " NST " + nst.NstId + " " + nst.name + " <color=red><b> empty buffer, copying current frame to </b></color>" + NextFrame.frameid +" \n curr: " + currentFrame));

				ExtrapolateNextFrame(svrWaitingForTeleportConfirm);

				return NextFrame;
			}

			extrapolationCount = 0;

			// First see if there is a future frame ready - ignoring late arrivles that may have backfilled behind the current frame
			Frame nextValid = GetFirstFutureValidFrame();

			// if not see if there is an older frame that arrived late, if so we will jump back to that as current
			if (nextValid == null)
			{
				nextValid = GetOldestPastValidFrame() ?? GetOldestValidFrame();

				// The only valid frames are only in the past, we need to jump back to the oldest to get our current frame in a better ballpark
				if (nextValid != null)
				{
					DebugX.Log(!DebugX.logInfo ? "" :
					//Debug.Log(
						(Time.time + " NST " + nst.NstId + " " + nst.name + " <color=red><b> Skipping back to frame </b></color> " + nextValid.frameid + " from current frame " + CurrentIndex));

					nextValid.CompletePosition(currentFrame);
					return nextValid;
				}
				
			}
			// Find out how far in the future the next valid frame is, need to know this for the reconstruction lerp.
			int stepsFromLast = CountFrames(CurrentIndex, nextValid.frameid);

			// The next frame is the next valid... not much thinking required... just use it.
			if (stepsFromLast == 1)
			{
				InvalidateOldFrames(NextFrame); // LIKELY UNEEDED
				NextFrame.CompletePosition(currentFrame);

				return NextFrame;
			}

			// if next frame on the buffer is a couple ahead of current, jump forward
			if (stepsFromLast > jumpForwardThreshold)
			{
				//Debug.Log(
				DebugX.Log(!DebugX.logInfo ? "" : 
					("<color=red><b>Jumping forward frame(s) </b></color>"));

				InvalidateOldFrames(nextValid);
				nextValid.CompletePosition(currentFrame);
				return nextValid;
			}

			//All other cases we Reconstruct missing next frame using the current frame and a future frame
			Frame next = NextFrame;

			//Debug.Log(
			DebugX.Log(!DebugX.logInfo ? "" : 
				(Time.time + " NST:" + nst.NstId + " <color=black><b>Reconstructing missing packet " + next.frameid + " </b></color> \n" + currentFrame.compPos + "\n" + nextValid.compPos));

			next.state = currentFrame.state;

			float t = 1f / stepsFromLast;

			nextValid.CompletePosition(currentFrame);

			Vector3 lerpedPos = Vector3.Lerp(currentFrame.rootPos, nextValid.rootPos, t);

			float lerpedStartTime = Mathf.Lerp(currentFrame.packetArriveTime, nextValid.packetArriveTime, t);

			next.ModifyFrame(currentFrame.updateType, currentFrame.rootSendType, lerpedPos, GenericX.NULL, lerpedStartTime);

			DebugX.Log(!DebugX.logInfo ? "" : 
				(Time.time + "fid" + next.frameid + " <color=red><b> RECONSTRUCT ELEMENTS </b></color> " + next.RootRot + " " + currentFrame.RootRot + " " + nextValid.RootRot));

			// Notify all interested components that they need to reconstruct a missing frame (elements and such)
			foreach (INstOnReconstructMissing callbacks in nst.iNstOnReconstructMissing)
				callbacks.OnReconstructMissing(next, currentFrame, nextValid, t, svrWaitingForTeleportConfirm);

			return next;
		}

		/// <summary>
		/// Marks all frames before the startingFrame as invalid
		/// </summary>
		public void InvalidateOldFrames(Frame startingframe)
		{
			for (int i = -quaterBufferSize; i < 0; i++)
			{
				SetBitInValidFrameMask(IncrementFrame(startingframe, i).frameid, false);
			}
		}

		/// <summary>
		/// Checks ENTIRE buffer for the oldest arriving frame. Used for the starting up.
		/// </summary>
		/// <returns>Returns null if no valid frames are found.</returns>
		public Frame GetOldestValidFrame()
		{
			if (validFrameMask <= 1)
				return null;

			float timetobeat = Time.time;
			int winnerwinnerchickendinner = 0;

			// First look forward
			for (int i = 1; i < bufferSize; i++)
			{
				//Frame testframe = IncrementFrame(CurrentIndex, i);
				if (GetBitInValidFrameMask(i) && frames[i].packetArriveTime < timetobeat)
				{
					winnerwinnerchickendinner = i;
					timetobeat = frames[i].packetArriveTime;
				}
			}
			return frames[winnerwinnerchickendinner];
		}

		/// <summary>
		/// Looks for farthest back valid frame before the current frame, starting with a quater buffer length behind working up to the current frame.
		/// </summary>
		/// <returns>Returns null if none found.</returns>
		public Frame GetOldestPastValidFrame()
		{
			for (int i = -quaterBufferSize; i < 0; i++)
			{
				Frame testframe = IncrementFrame(CurrentIndex, i);
				if (GetBitInValidFrameMask(testframe.frameid))
				{
					return testframe;
				}
			}
			return null;
		}

		/// <summary>
		/// Get the first valid frame BEFORE the current frame.
		/// </summary>
		public Frame GetNewestPastValidFrame()
		{
			for (int i = -1; i >= -quaterBufferSize; i--)
			{
				Frame testframe = IncrementFrame(CurrentIndex, i);
				if (GetBitInValidFrameMask(testframe.frameid))
				{
					return testframe;
				}
			}
			return null;
		}

		/// <summary>
		/// Looks for first valid frame AFTER the current frame.
		/// </summary>
		/// <returns>Returns null if none found.</returns>
		public Frame GetFirstFutureValidFrame()
		{
			for (int i = 1; i <= quaterBufferSize; i++)
			{
				Frame testframe = IncrementFrame(CurrentIndex, i);
				if (GetBitInValidFrameMask(testframe.frameid))
				{
					return testframe;
				}
			}
			return null;
		}

		public Frame GetFurthestFutureValidFrame(Frame startingframe = null)
		{
			if (startingframe == null)
				startingframe = currentFrame;

			for (int i = quaterBufferSize; i > 0; i++)
			{
				Frame testframe = IncrementFrame(startingframe, i);
				if (GetBitInValidFrameMask(testframe.frameid))
				{
					return testframe;
				}
			}
			return null;
		}

		public string PrintBufferMask(int hilitebit = -1)
		{
			string str = "";
			string tagsEnd = "";

			for (int i = bufferSize - 1; i >= 0; i--)
			{
				tagsEnd = "";
				if (frames[i].rootSendType.IsLBits())
				{
					str += "<color=orange>";
					tagsEnd = "</color>" + tagsEnd;
				}
				else if (frames[i].rootSendType.None())
				{
					str += "<color=blue>";
					tagsEnd = "</color>" + tagsEnd;
				}
				if (i == CurrentIndex)
				{
					str += "<b>";
					tagsEnd = "</b>" + tagsEnd;
				}

				str += (BitTools.GetBitInMask(validFrameMask, i)) ? 1 : 0;

				str += tagsEnd;

				if (i % 4 == 0)
					str += " ";
			}
			return str + " orng=lbits, blue=no_pos";
		}
	}
}
