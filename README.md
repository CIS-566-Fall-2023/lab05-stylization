# lab05-stylization
Let's practice using Unity's shader graph!

## 1. Introduction
  a. We will be creating a toon shader in Unity that supports shadows and multiple lights in real-time! In the process, you will hopefully get a little bit of familiarity with Unity’s shader graph!

## 2. What’s provided:
  1.  The tutorial video I record for the lab will go over the base code, and will then go over the process of making a limited version of the final toon shader. 
  2.  Full Requirements of the shader:
    1.  (ATLEAST) 3 Different Bands of Colors based on the light/normal dot product:
      1. Highlight, Midtone and Shadow.
    2. Has adjustable threshold values for these color bands
    3. Has a smoothness parameter for interpolation between color bands
  3. Limited Version:
      1. Has 2 bands of colors, black and a customizable albedo
## 3. Lab Puzzles/Goals:
  1. The goal of each puzzle will be to replicate the look of each puzzle’s image
  2. Puzzle 1: A scene with a bunch of 2 color toon shaded objects
     1. Student just needs to follow the tutorial to create a 2 band toon shader, and then create multiple materials based off of the shader graph, and then attach that material to the objects in the default scene.
  3. Puzzle 2: A scene with 3-band toon shaded objects
     1. Self-explanatory: Students will need to allow for a 3rd color in their scene, and ALSO, implement adjustable thresholds such that they can vary the level of shadow.
  4. Puzzle 3: Customized Shadow Pattern
     1.  Students will use one of the provided texture png’s in order to add a screenspace shadow pattern onto the shadows of the scene!

