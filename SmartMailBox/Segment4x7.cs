using System;
using System.Collections.Generic;
using System.Text;
using Tinkerforge;

namespace SmartMailBox
{
    static class Segment4x7
    {
        private static string HOST = "localhost";
        private static int PORT = 4223;
        private static string UID = "iV3"; // Change to your UID!

        public static void ShowSegmentText()
        {
            IPConnection ipcon = new IPConnection(); // Create IP connection
            BrickletSegmentDisplay4x7 sd4x7 = new BrickletSegmentDisplay4x7(UID, ipcon); // Create device object

            ipcon.Connect(HOST, PORT); // Connect to brickd
            // Don't use device before ipcon is connected

            byte[] POST = { 0x73, 0x3f, 0x6d, 0x78 };

            // Write content
            byte[] segments = { POST[0], POST[1], POST[2], POST[3] };
            sd4x7.SetSegments(segments, 0, false);

            ipcon.Disconnect();
        }

        public static void TurnOffSegment()
        {
            IPConnection ipcon = new IPConnection(); // Create IP connection
            BrickletSegmentDisplay4x7 sd4x7 = new BrickletSegmentDisplay4x7(UID, ipcon); // Create device object

            ipcon.Connect(HOST, PORT); // Connect to brickd
            // Don't use device before ipcon is connected

            byte[] segments = { 0x0, 0x0, 0x0, 0x0, };

            sd4x7.SetSegments(segments, 0, false);

            ipcon.Disconnect();
        }

        public static void ShowLock()
        {
            IPConnection ipcon = new IPConnection(); // Create IP connection
            BrickletSegmentDisplay4x7 sd4x7 = new BrickletSegmentDisplay4x7(UID, ipcon); // Create device object

            ipcon.Connect(HOST, PORT); // Connect to brickd
            // Don't use device before ipcon is connected

            byte[] segments = { 0x3f, 0x3f, 0x3f, 0x3f, };

            sd4x7.SetSegments(segments, 0, false);

            ipcon.Disconnect();
        }

    }
}
