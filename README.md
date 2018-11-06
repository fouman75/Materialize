## Synopsis

Materialize is a stand alone tool for creating materials for use in games from images. You can create an entire material from a single image or import the textures you have and generate the textures you need.

## How to use
- Open (O) a texture in the diffuse box.
- Create Height map from Diffuse
- Create Normal from Height map
- ... Create maps as needed ...

## OSX

- Application was built using Unity 2018.2.14f1 on OSX 10.13.6 (High Sierra)
- Application can run directly with .app or within Unity

### Limitations / Changes

- OSX branch should not be merged into master. Altough platform specific #if were used on the UI, a lot of code was also removed. The goal was to make it work quickly on OSX without necessarily thinking about maintaining the base code as fully multiplatform.
- Dependency on FreeImage was removed due to .dll not being found on Mac. I'm not a Unity expert on .net dlls on OSX. It was just simpler to refactor the code with another FileBrowser and remove some functionality.
- Copy/Paste functionality has been removed.
- Diffuse map moved to first as it's the starting point of the process.
- Only png and jpg image formats are supported.

## License

Materialize is open source under the GNU GPL v3.
