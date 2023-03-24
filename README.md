# AddCoverToVideoFile
A simple cross-platform desktop GUI app to easily add cover art/thumbnail to MP4 and MKV files.  

This app is developed as an [Avalonia UI](https://avaloniaui.net/) app and uses [Taglib#](https://github.com/mono/taglib-sharp) for metadata tag editing, so it should run and work on Windows, Linux, and Mac as well using .NET runtime. 

However, it turnes out [Avalonia UI currently does not support file drag and drop on Linux](https://github.com/AvaloniaUI/Avalonia/issues/6085) ... so it runs fine but it does not accept file drop on Linux... Sad. You have to manually select files using a file open button on Linux.

In order to show video thumbnails/cover pictures in Ubuntu's file manager "Nautilus/GNOME Files", you need to "sudo apt install ffmpegthumbnailer" and configure it to read metadata picture. The config file is loacaed "/usr/share/thumbnailers/ffmpegthumbnailer.thumbnailer" and add -m option to the exec options like this: "Exec=ffmpegthumbnailer -i %i -o %o -s %s -f -m". In windows, MP4's picture is shown normaly but MKV is not supported. 

Also, what I learned is that metadata "tags" for movie files in general are pretty messed up. There is no standard at all. This is beyond sad state.  

## Remark
The project file is targeting Avalonia version="11.0.0-preview6" (current). 

## Download
TODO

## Screenshots

![AddCoverToVideoFile](https://github.com/torum/AddCoverToVideoFile/blob/main/files/screenshots/screenshots.jpg?raw=true)
