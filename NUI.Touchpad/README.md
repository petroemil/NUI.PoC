# Touchpad v2

This is a different approach to the touchpad that we all know very well with all of its limitations.

I wanted to design an interaction model that is closer to a real mouse and enables some interactions that were not possible with a touchpad, or if they were, they were a bit awkward.

So, here is the idea: Use the Middle finger as a pointer, and use the Index finger and the Ring finger as left and right buttons.

---

The idea is that one finger only moves the cursor (no accidental taps or dropping a dragged piece of content because you accidentally ran off the touchpad surface), and then the second finger to the left will act as the left mouse button, and the second finger to the right will act as the right mouse button. Using either the mouse buttons no longer require the finger to be lifted (and a "tap" gesture to be made), or an awkward pose to use the physical button on the bottom of the touchpad with your thumb.

As a very first step, I made a pretty raw PoC app to at least build the most basic functionality, to process touch events and detect the different fingers. It's pretty simple stuff, but a well organised codebase to extend.

To make it easy to use on any Windows 10 tablet or phone, I made this app as an UWP app. Maybe for even better portability, I will port it to Xamarin.Forms.