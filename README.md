# vgy.me
![.NET Core](https://github.com/Merlin04/vgy.me/workflows/.NET%20Core/badge.svg)

A CLI for the image hosting website vgy.me. To use this, you'll need a vgy.me userkey, which you can generate [here](https://vgy.me/account/details). 

## Usage

### Upload

Upload an image:
```
$ vgy.me upload file1.png
URL: https://vgy.me/u/demo
Image URL: https://i.vgy.me/demo.png
Delete: https://vgy.me/delete/democode
```

Upload an image with a title and description:
```
$ vgy.me upload file2.png -t "Title" -d "Description"
```

Upload multiple images to an album:
```
$ vgy.me upload file1.png file2.png file3.png
Album URL: https://vgy.me/album/demo
Images:
 - https://i.vgy.me/demo1.png
 - https://i.vgy.me/demo2.png
 - https://i.vgy.me/demo3.png
```

### Delete

Delete an image using the delete URL:
```
$ vgy.me delete https://vgy.me/delete/democode
```

Delete an image using the viewer URL or image URL:
```
$ vgy.me delete https://vgy.me/u/demo
```

Delete all images uploaded since the app was installed (does not include images uploaded as part of an album):
```
$ vgy.me delete all
```

### Configure

Set userkey:
```
vgy.me configure "insert-userkey-here"
```

Remove configuration file:
```
vgy.me configure reset
```

## How it works
This tool is written in C#/.NET Core and uses the CliFx framework, which provides all of the behavior of command line apps you're used to like help screens and arguments. It also provides a way to implement dependency injection, which allows all of the commands to get access to an `HttpClient` and the configuration data, which is stored in `~/.vgy.me.json` and is loaded when the app is run. Uploading images uses the vgy.me API, and deleting them uses `ScrapySharp` to access the delete page and click the delete button. The tool keeps track of the files you upload in the configuration file and can use that data to look up the delete URL for an image URL. Unfortunately, the vgy.me API does not provide a delete URL for albums so the tool can't keep track of those, you have to delete them from the web interface.
