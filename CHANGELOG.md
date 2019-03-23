# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [v0.4HDRP-beta.7] - 2019-xx-xx
### Added
- Added Reset to Defaults

### Changed

#### Minor changes
- Update to SRP 5.10.0
- Minor UI Adjusts
- Update TextMeshPro to 2.0.0

### Fixes


### To Implement
- Windows Copy / Paste
- Linux Copy to clipboard


## [v0.4HDRP-beta.6] - 2019-23-03
### Added
- Added Support for Copy/Paste in nautilus 3.26.
- Added Info Messages on screen.
- Added Graphics Quality Selection.
- Added support for 4:3 screens.

### Changed
- Start in rhe second highest resolution available.
- Change to full material view when the windows are closed.

#### Minor changes
- Update to Unity 2019.1.0b8
- Update to SRP 5.9.0

### Fixes
- Fix Full Screen.
- Adjust Post Processing box size when disabling itens.
- Fix paste of non power of two textures.
- Better thread groups handling in cases where the texture resolution is odd.
- Speed improve in processing.
- Don't let user be too fast pressing buttons and breaking things.
- Other stability changes to Compute Shaders.
- Fix Normal Packing in Material View.
- Fix gama correction when saving.
- Fix Post Processing enable disabling other than not Post Processing Effects.
- Minor fixes and improve stability.

### To Implement
- Windows Copy / Paste
- Linux Copy to clipboard


## [v0.4HDRP-beta.5a] - 2019-13-03
### Changed since beta 5
- New Options to Post Processing.
- Better Gui Scaling to big windows.
- Hack to fix other aspect ratios that not 16:9.
- Fix Textures Set when they are empty.

## [v0.4HDRP-beta.5] - 2019-13-03
### Added
- Added Slider from NormalMap in AO Map Creation.
- Added Depth of Field to the Scene.
- Added Screen Space Reflections.

### Changed
- Migrated all texture processing shaders from frag/vert shaders to Compute Shaders. Everything is looking great for me, performance is also great.

#### Minor changes
- Lots of adjusts to post processing effects.

### Fixes
- Fix Smoothness Map, was behaving the oppose to expected.
- Fix Mask Map when any texture is missing.
- Fix Panning when panning not from centre.
- Fix a problem with object getting stuck when zooming too much.
- Fix Save Texture modifying original texture.
- Minor fixes and improve stability.

### Known Issues
- Full Screen is not working, Unity Bug
- Quick-save is not implemented yet.

## [v0.4HDRP-beta.4] - 2019-05-03
### Changed since beta 3a
- Remove Gama Correction from all shaders.
- Revert gama correction from last build.
- Finally normals are being showed right, no right clue on internet, needed to reverse engineer the HDRP normal packing (they are all red = 1 now).
- Using MipMaps for better scalling

### Known Issues
- Full Screen is not working, Unity Bug
- Quick-save is not implemented yet.

## [v0.4HDRP-beta.3a] - 2019-05-03
### Fixes
- Fix preview saturation
- Fix plane rotation (Was upside down)

## [v0.4HDRP-beta.3] - 2019-05-03
### Added
- Add button to reset position and rotation.

#### Mitch Ideas implemented
- Implemented FrameRate Choice in settings.
- Implemented option to don't hide on rotate.

### Changed
- Downgrade from 2019.2 to 2019.1b5
- Change HDRP to 5.6.1
- Better looking Box.
- New UI Theme, cleaner
- Frame idependent Rotate / Pan / Zoom
- Change normal to Object Space normal intead of Tangent Space. Experiment.

#### Minor changes
- Scene enhanced with better Reflection
- Rise precision of Mask Map processor shader.
- Rise precision of Linear to Gama shader.
- Created Shader Variants pack, to force unity to keep the actual shaders.
- Zoom now is smooth. Very nice.
- New Shader PackNormal, not being used for now.

### Fixes
- Fix a problem that when manipulating or hiding we were losing the actual processing.
- Fix to Reset Position/Rotation.
- Tesselation center now is correct.
- Vsync was causing freezes, then was disabled for now.
- Use Lerp instead of Slerp to position. Fixes undesirable zoom.
- Other Fixes;

### Known Issues
- Full Screen toggle not working, Unity Bug.
- Quick-save is not implemented yet.

## [v0.4HDRP-beta.2] - 2019-23-02
### Fixes
- Fix a problem with clearing textures
- Fix Hide while Rotating/Panning

### Known Issues
- Quick-save is not implemented yet.

## [v0.4HDRP-beta.1] - 2019-23-02
### Added
- New Texture Type, MaskMap, HDRP uses this texture as AO, Smoothness and Metallic, then, after modify any of these, recreate the MaskMap to see the change in Test Object.

### Changed
#### High Definition Render Pipeline
- Materialize now uses HDRP 6.4.0, modified to work in Latest Unity Editor, 2019.2a4.
- Tons of changes were needed, new shaders, changes in colour space handling and a lot more.

#### HDRP Post Processing
- Post Processing now is the new Unity Post Processing 3.0
- GUI for it is basic for now, i will add more options later.

#### UI
- New UI, with auto scale, then no more problems with all the different aspect ratios and resolutions.
- Apply now is in the same place as create, allowing quick create using the default values. Saves space too.

#### Minor changes
- Changed to Unity 2019.2a4
- New Pan, Rotate and Zoom.
- Zoom is at mouse position, while rovers the object.
- Frame Rate is now limited to 30 fps,, will include on Settings GUI later.

### Fixes
- Fixes for TGA and EXR.
- Other Fixes;

### Known Issues
- Quick-save is not implemented yet.
