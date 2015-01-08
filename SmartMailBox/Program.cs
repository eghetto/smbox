using System;
using System.Collections.Generic;
using System.Text;
using Tinkerforge;
using System.Net;
using System.Threading;

namespace SmartMailBox
{
	class Program
	{
		private static string HOST = "localhost";
		private static int PORT = 4223;
		private static string UID = "jB1"; // Change to your UID!

		private const string FHEMADDRESS = "http://192.168.0.127:8083";
		private const int lockTimeSeconds = 5;

		private static bool lockable = true;

		private enum State { off, on, locked };
		private static State _state;
		private static State CurrentState {
			get 
			{
				return _state; 
			}
			set 
			{
				if (_state != value) 
				{
					_state = value;
					System.Console.WriteLine(DateTime.Now + ": New State:" + value.ToString());
					Notify();
				}
			} 
		}


		static private void RunLockCountDown(int lockSeconds)
		{
			if (lockable == true)
			{
				CurrentState = State.locked;
				Segment4x7.ShowLock();
				var stopTime = System.DateTime.Now.AddSeconds(lockSeconds);
				while (System.DateTime.Now <= stopTime)
				{
					System.Console.WriteLine(DateTime.Now + ": Lock!");
					System.Threading.Thread.Sleep(1000);
				}
				Segment4x7.TurnOffSegment();
				System.Console.WriteLine(DateTime.Now + ": Unlocked!");
				lockable = false;
				CurrentState = State.off;
			}
		}


		// Callback for distance changes
		static void ReachedCB(BrickletDistanceUS sender, int distance)
		{
			if (CurrentState != State.locked)
			{
				if (distance < 550 || distance > 630)
				{
					//System.Console.WriteLine(DateTime.Now + ": Distance value out of range: " + distance);
					CurrentState = State.on;
					Segment4x7.ShowSegmentText();
				}
				else
				{
					CurrentState = State.off;
					Segment4x7.TurnOffSegment();
				}
			}
		}

		// notify if box contains new mail
		static void Notify()
		{
			switch (CurrentState)
			{
				case State.off:
					//SendFhemCommand("SetSmartMailBoxOff();;");
					RunLockCountDown(lockTimeSeconds);
					break;
				case State.on:
					//SendFhemCommand("SetSmartMailBoxOn();;");
					lockable = true;
					break;
				case State.locked:
					//SendFhemCommand("SetSmartMailBoxLocked();;");
					break;
			}
		}

		// send command to FHEM
		static void SendFhemCommand(string fhemCommand)
		{
			//System.Console.WriteLine(DateTime.Now + ": Notifying...");
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(FHEMADDRESS + "/fhem?cmd={" + fhemCommand + "}");
				WebResponse response = request.GetResponse();
				request = null;
				response = null;
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(DateTime.Now + ": Error - " + ex.ToString());
			}
		}

		static void Main()
		{
			System.Console.WriteLine(DateTime.Now + ": Starting... ");
			IPConnection ipcon = new IPConnection(); // Create IP connection
			BrickletDistanceUS dir = new BrickletDistanceUS(UID, ipcon); // Create device object

			Segment4x7.TurnOffSegment();

			ipcon.Connect(HOST, PORT); // Connect to brickd
			// Don't use device before ipcon is connected

			// Get threshold callbacks with a debounce time of 1 second (1000ms)
			dir.SetDebouncePeriod(1000);

			// Register threshold reached callback to function ReachedCB
			dir.DistanceReached += ReachedCB;

			// Configure threshold 
			dir.SetDistanceCallbackThreshold('o', 0, 1);

			// Setup and start timer...
			var mre = new ManualResetEvent(false);

			// allow the code to exit from the command line:
			ThreadPool.QueueUserWorkItem((state) =>
			{
				//Console.WriteLine("Press (x) to exit");
				while (true)
				{
					var key = Console.ReadKey();
					if (key.Key == ConsoleKey.X)
					{
						mre.Set(); // This will let the main thread exit
						break;
					}
				}
			});

			System.Console.WriteLine(DateTime.Now + ": Running... (hit 'x' to exit)");

			// The main thread can just wait on the wait handle, which basically puts it into a "sleep" state, and blocks it forever
			mre.WaitOne();

			System.Console.WriteLine();
			System.Console.WriteLine(DateTime.Now + ": Exiting!");
			ipcon.Disconnect();

			Segment4x7.TurnOffSegment();
		}
	}
}
