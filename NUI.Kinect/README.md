# Touchpad v2

My first experiment is a slightly different approach to the touchpad that we all know very well with all of its limitations.

I wanted to design an interaction model that is closer to a real mouse and enables some interactions that were not possible with a touchpad, or if they were, they were a bit awkward.

So, here is the idea: Use the Middle finger as a pointer, and use the Index finger and the Ring finger as left and right buttons.

When only one finger is down, its only going to move the cursor, no accidental taps or dropping a dragged piece of content because we accidentally ran off the touchpad surface.

Using either the left or right mouse button doesn't require the finger to be lifted (and a "tap" gesture to be made), or an awkward pose to use the physical button on the bottom of the touchpad with our thumb.

As a very first step, I made a pretty raw PoC app to at least build the most basic functionality, to process touch events and detect the different fingers.