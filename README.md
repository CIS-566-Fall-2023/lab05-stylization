# lab05-stylization
Let's practice adding stylization to a 3D scene using Unity's shader graph!

## 1. Introduction
We will be stylizing a "toon" look by creating a shader in Unity that supports shadows and multiple lights in real-time! In the process, you will gain some familiarity with Unity’s shader graph.

## 2. What’s provided:
This tutorial video will cover the base code, and then go over the process of making a limited version of a toon shader. 
  2.  Full Requirements of the shader:
    1.  (ATLEAST) 3 Different Bands of Colors based on the light/normal dot product:
      1. Highlight, Midtone and Shadow.
    2. Has adjustable threshold values for these color bands
    3. Has a smoothness parameter for interpolation between color bands
  3. Limited Version:
      1. Has 2 bands of colors, black and a customizable albedo
         
## 3. Lab Puzzles/Goals:
The goal of each puzzle will be to replicate the look of each puzzle’s image.

  2. Puzzle 1: Simple two-tone toon shading [IMAGE]
     1.  Follow the tutorial to create a 2 band toon shader, and then create multiple materials based off of the shader graph, and then attach those materials to the objects in the default scene.
  4. Puzzle 2: Leveled-up toon shading [IMAGE]
     1. Edit your materials to allow for a 3rd color in your scene, such that you have highlights, midtones, shadows on your objects. Edit your shader so that the thresholds on these values are adjustable.
     2. Add some soft interpolation at the edges of your bands, for smooth transitions between color bands. Create a "smoothness" parameter that adjusts the degree of smoothness!  
  5. Puzzle 3: Stylized Shadow
     1.  Use one of the provided texture png’s in order to add a screenspace shadow pattern onto the shadows of the scene!

