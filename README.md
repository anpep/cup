
![version: 0.5](https://img.shields.io/badge/version-0.5-blue.svg)
![license: GNU GPL v2](https://img.shields.io/badge/license-GNU_GPL_v2-limegreen.svg)
> 🎞 Simple screen capturing tool. Captain predecessor.

## What's this?
It's a small utility to take screenshots. It's compatible with [ShareX custom uploaders](https://github.com/ShareX/CustomUploaders)
and lets you set key bindings for each action. You can also create your own actions using simple JSON files.

## Building from source
### Environment Requirements
- Windows 7 or newer (build only tested on latest Windows 10)
- [Visual Studio 2015](https://www.visualstudio.com/downloads/) or newer (Community edition is fine)

### Cloning
The project is split in git submodules, so don't forget the `--recursive` flag when cloning.

```
$ git clone --recursive https://github.com/anpep/cup
$ cd cup
```

### Building
Then you can either open `cup.sln` on Visual Studio or build it from the command-line:
```
$ nuget restore
$ devenv cup.sln /Build
```

## Licensing
This software is made up of different components that may not necessarily be licensed under the same terms.
Refer to the `LICENSE.md` file in the top-level directory of each project to find further details about their
licensing.

Major components are licensed under the [GNU GPL version 2](https://www.gnu.org/licenses/old-licenses/gpl-2.0.en.html),
with some components licensed under the [BSD License](http://www.linfo.org/bsdlicense.html).
