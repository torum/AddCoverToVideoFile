# AddCoverToVideoFile
A simple cross-platform desktop GUI app to easily add cover art/thumbnail to MP4 and MKV files.  

This app is developed as an [Avalonia UI](https://avaloniaui.net/) app and uses [Taglib#](https://github.com/mono/taglib-sharp) for metadata tag editing, so it should run and work on Windows, Linux, and Mac as well using .NET runtime. 

In order to show video thumbnails/cover pictures in Ubuntu's file manager "Nautilus/GNOME Files", you need to "sudo apt install ffmpegthumbnailer" and configure it to read metadata picture. The config file is loacaed "/usr/share/thumbnailers/ffmpegthumbnailer.thumbnailer" and add -m option to the exec options like this: "Exec=ffmpegthumbnailer -i %i -o %o -s %s -f -m". In windows, MP4's picture is shown normaly but MKV is not supported. 

## Remark
The project file is targeting Avalonia version="12.1.0" with Visual Studio's Avalonia add-on.

## Download
TODO

## Screenshots

![AddCoverToVideoFile](https://github.com/torum/AddCoverToVideoFile/blob/main/files/screenshots/screenshots.jpg?raw=true)
