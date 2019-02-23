# Materialize
Materialize is a program for converting images to materials for use mainly but not restrict to video games.

## Motivation
I decided to port materialize to linux, since the original is for windows only. I will keep improving it.

## Contact
For sugestions, doubts or anything related to this port.
- Email : mk2play.materialize@gmail.com

## Using
To use, unity is not necessary, you can use like a normal linux application.

## Building
I'm developing using Unity 2019.2a4, then, is recomended to use it also. I'm using a local HDRP modified, v6.4.0, then, wont be easy to build unless you try to add any version v6.0.0 and above, wont work out of the box, since they use a not launched editor version. I wull include the modded HDRP in a later version.

You can try to downgrade or upgrade the package, but mainly downgrading, something can go wrong.

## About HDRP Version
- I'm making this version thinking in the future, since editor and HDRP are in development yet.
- You can use the Mask Map texture in any version of HDRP, only to build by yourself that you can have troubles. 
- Even HDRP not using separate smoothness, ao and metallic textures, i decided to keep, since you may want to use these textures with other programs.

## Added features
### Paste Images from clipboard on Linux
- You can copy a file in your file browser (Tested with nautilus) and then press  the "P" close to the slot you want to paste.
- **Highlight** - You can also press copy image on browser and it will paste also. This make it fast to take a image from internet
### Hide Gui while Rotating / Panning
- The GUI is hidden when panning/rotating the material plane.
### Native File Picker
- Added a new native file/folder picker - Unity Standalone File Browser - https://github.com/gkngkc/UnityStandaloneFileBrowser - Thanks to @gkngkc for the amazing work.
 
## Changed from original
### Save and Load Project
- When you save your project, every map will be saved in the same place, with them respective types, ex:myTexture_Diffuse.png.
- The extension used will be the one set in the GUI Panel.
#### Suported extensions
##### Save
- jpg
- png
- tga
- exr

##### Load
- jpg
- png
- tga
- bmp

## Not implemented
- QuickSave - Will implement in settings, then you can set the folder to save the texture. This will be a persistent setting, that means you can close and open the program without lose the Quick Save path. *Planed for v0.4*.
- Copy to clipboard. *Planed for v0.4*.
