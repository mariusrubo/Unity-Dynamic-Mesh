# Unity-Dynamic-Mesh
An example of a procedural mesh which can be changed dynamically along several parameters at runtime

# Purpose
3D objects are usually loaded into Unity using formats like FBX. This approach allows to alter an object's mesh by means of animations, bones or blendshapes. In this project, on the other hand, we generate an object via script, which allows to completely procedurally alter some of its basic properties at runtime. 

![alt tag](https://github.com/mariusrubo/Unity-Dynamic-Mesh/blob/master/changing_structure.gif)

If the Gif does not load, see the image "changing_structure.jpg" in this repo. 

# Installation
The mesh seen on the image is entirely created using a single script ("treehouse.cs") which you can attach to any object in your Unity project. All you need to add is a material of your choice (I used a material from the package "Procedural Simple Wood Texture (PBR)". Note that you also need to import the asset "Substance in Unity" for this to work in newer Unity versions).

The script includes a class which allows to define prisms with a hexagonal base by specifying all 12 corner positions. The entire structure is then made up of such prisms, and the corner points are defined as functions of a few parameters. Changing these parameters results in changes of shape as seen in the gif. 

# Further Reading
As a starting point into procedural meshes in Unity I recommend these two youtube tutorials:
* https://www.youtube.com/watch?v=eJEpeUH1EMg
* https://www.youtube.com/watch?v=64NblGkAabk&t=600sfree 
