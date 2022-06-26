# Texture Converter

This program will scan all folders under texture_sources and convert any .tga, .png, .jpg or .dds files it finds to .texture format, mirroring the directory structure into texture_library.

By default, it will call nvcompress.exe using the -bc1 compression setting, which is good for world textures and creates mipmaps. It will also set the texture width and height to whatever the original image size is.

The tool will not attempt to recompress .dds files, it will instead copy them as-is and just attach the .texture header.
