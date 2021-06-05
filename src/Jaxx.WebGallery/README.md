## User Roles
User have to authenticate with a role.

|GalleryUser|Allows usage of GalleryController API.|
|GalleryAdmin|Allows usage of GalleryAdmin API.|

## Configuration
App should have a configuration json containing the following settings:

### WebGallery::Database::ConnectionString
Connection string to gallery database

### WebGallery::AlbumThumbPath
The local path, where the generated album thumbs are saved to and loaded from. This path should not be readable for web server.

### WebGallery::ImageThumbPath
The local path, where the generated image thumbs are saved to and loaded from. This path should not be readable for web server.

### Example Json

```
{
  "WebGallery": {
    "Database": {
      "ConnectionString": "server=192.168.0.xx;userid=userid;pwd=secret;port=3306;database=gallerydb;sslmode=none;"
    },
    "AlbumThumbPath": "./cache/albumThumbs/",
    "ImageThumbPath": "./cache/imageThumbs/"
  }
}
```