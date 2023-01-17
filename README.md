# AddCoverToVideoFile
A simple cross-platform desktop GUI app to easily add cover art/thumbnail to MP4 and MKV files.  

This app is developed as an [Avalonia UI](https://avaloniaui.net/) app and uses [Taglib#](https://github.com/mono/taglib-sharp) for metadata tag editing, so it should run and work on Windows, Linux, and Mac as well using .NET runtime. 

However, it turnes out [Avalonia UI currently does not support file drag and drop on Linux](https://github.com/AvaloniaUI/Avalonia/issues/6085) ... so it runs fine but it does not accept file drop on Linux... Sad. You have to manually select files using a file open button on Linux.

Also, what I learned is that metadata "tags" in movie files are pretty messed up. There is no standard at all. This is beyond sad state.  

## Download
TODO

## Screenshots

![AddCoverToVideoFile](https://github.com/torum/AddCoverToVideoFile/blob/main/files/screenshots/screenshots.jpg?raw=true)

![AddCoverToVideoFile](https://github.com/torum/AddCoverToVideoFile/blob/main/files/screenshots/screenshots2.png?raw=true)
