﻿# AirTouch with Kinect

Have you ever tried to move the hand cursor on an Xbox? Did it feel natural? Did you have the feeling that you are in control of the situation?

I was thinking, why can't it be simpler, more natural?

So I came up with a different method to move a cursor on the screen with your hand.

The basic assumption is that when you try to point to something on the screen, you will raise your index finger and place it in front of you, so it will cover (from your perspective) the piece of UI that you are pointing to. Translating it to a more mathematical definition, the cursor should be projected on the screen where the 3D line defined by your eye and index finger intersects the plane of the display.

As a first iteration I just made a simple application that does nothing more than showing a cursor where you are pointing at the screen. It does the only the most basic and most necessary things, but it's a good foundation to build on top of.
