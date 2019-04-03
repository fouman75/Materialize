# Materialize
Materialize is a program for converting images to materials for use mainly but not restrict to video games.

## Motivation
I decided to port materialize to linux, since the original is for windows only. I will keep improving it.
There are builds for windows also and soon i will give it a bit more support. Focus still in linux.

## System Requirements
- Latest Graphics drivers - Nvidia 418 recomended.
- Vulkan support in linux.
- DirectX 11/12 in windows, or vulkan.
- If with nvidia, dont use Nouveau since it will for sure crash with vulkan.
- Nvidia on wayland is not supported.
- Ubuntu 16.04+;
- Windows 7+.

## Contact
For sugestions, doubts or anything related to this port.
- Email : mk2play.materialize@gmail.com

## Using
To use, Unity is not necessary, you can use like a normal linux application.

### Basic Usage
- Load a texture in the diffuse (Mandatory)
- Create or load the maps you need.

#### Creating
- The button create will only be available when the needed map is available. (Ex.: Height map requires diffuse.)
- Press create, adjust your values then press apply to create the image.

#### Saving Project
- When you save the project, all the textures will be saved in that folder together with the .mtz file.
- The textures will be automatically named following the map type. (Ex.: mymap_Height.png, mymap_Diffuse.png)

#### Loading Project
- Just load your .mtz.
- Textures must be in the same folder for them to load.
- Don't rename the textures.

#### Quick Saving
- After you save using the save button in the panel, quick save will save and overwrite to that location.
- If you or load the project, quick save will overwrite the project's texture file.
 
#### Saving Extension
- You can choose one of the supported extensions by simple naming the file. (Ex.: myTexture.jpg).
- When saving the project, the extension used will be the one selected in the save panel on the right.

#### Visualizing the Result
- Just press Show Full Material.

## Building
I'm developing using Unity 2019.1b9, then, is recomended to use it also. All you need to do is open in Unity and use it. It works in linux and windows without troubles related to the platform.

You can try to downgrade or upgrade the package, but mainly downgrading, something can go wrong.

## About HDRP Version
- I'm making this version thinking in the future, since editor and HDRP are in development yet.
- You can use the Mask Map texture in any version of HDRP. 
- Even HDRP not using separate smoothness, ao and metallic textures, i decided to keep, since you may want to use these textures with other programs.
- Uses Vulkan.
- Requires new hardware.

## Added features
### Paste Images from clipboard on Linux
- You can copy a file in your file browser (Tested with nautilus) and then press  the "P" close to the slot you want to paste.
- **Highlight** - You can also press copy image on browser and it will paste also. This make it fast to take a image from internet.

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

### Supported Screen Aspect Ratios
- 4:3
- 16:9
- 16:10

## Not implemented
- Copy to clipboard.
