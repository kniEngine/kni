﻿# Kni

Kni is a simple and powerful .NET framework for creating games for desktop PCs, and mobile devices, Virual Reality headsets and the Web, using [dotnet](https://dotnet.microsoft.com/) framework & the C# programming language. 

It is an open-source re-implementation of the discontinued [Microsoft's XNA Framework](https://en.wikipedia.org/wiki/Microsoft_XNA).
Kni is a derivative of [MonoGame & XNA Touch](https://github.com/MonoGame/MonoGame).

[![Join the chat at https://discord.gg/monogame](https://img.shields.io/discord/355231098122272778?color=%237289DA&label=MonoGame&logo=discord&logoColor=white)](https://discord.gg/monogame)

* [Build Status](#build-status)
* [Supported Platforms](#supported-platforms)
* [Support and Contributions](#support-and-contributions)
* [Source Code](#source-code)
* [Helpful Links](#helpful-links)
* [License](#license)

## Supported Platforms

We support a growing list of platforms across the desktop, mobile, and VR/AR space.  If there is a platform we don't support, please [make a request](https://github.com/MonoGame/MonoGame/issues) or [come help us](CONTRIBUTING.md) add it.

 * Windows 8.1 and up (OpenGL & DirectX11)
 * Windows Store Apps (UAP)
 * Oculus VR (OvrPC/DirectX11/meta link)
 * Oculus VR/AR (native/OpenGL)
 * Linux (OpenGL)
 * macOS 10.15 and up (OpenGL)
 * Android 6.0 and up (OpenGL)
 * iPhone/iPad 10.0 and up (OpenGL)
 * Web Browsers & WebXR (WebGL)

## Support and Contributions

If you think you have found a bug or have a feature request, use our [issue tracker](https://github.com/kniengine/kni/issues). Before opening a new issue, please search to see if your problem has already been reported.  Try to be as detailed as possible in your issue reports.

If you need help using Kni or have other questions we suggest you post on our [discussions forums](https://github.com/kniEngine/kni/discussions).  Please do not use the GitHub issue tracker for personal support requests.

If you are interested in contributing fixes or features to Kni, please read our [contributors guide](CONTRIBUTING.md) first.

## Source Code

The full source code is available here from GitHub:

* Clone the source: `git clone https://github.com/kniengine/kni.git`
* Set up the submodules: `git submodule update --init`
* Open the solution for your target platform to build the game framework.
* Open the Tools solution for your development platform to build the pipeline and content tools.

For the prerequisites for building from source, please look at the [Requirements](REQUIREMENTS.md) file.

A high level breakdown of the components of the framework:

* The core framework libraries are located in [src](src).
* The base content pipeline library is located in [src/Xna.Framework.Content.Pipeline](src/Xna.Framework.Content.Pipeline).
* The Design converters are located in [src/Xna.Framework.Design](src/Xna.Framework.Design).
* The platform backend implementations are found in [Platforms](Platforms).
* Project templates are in [Templates](Templates).
* See [Tests](Tests) for the framework unit tests.
* See [Tools/Tests](Tools/MonoGame.Tools.Tests) for the content pipeline and other tool tests.
* [mgcb](Tools/MonoGame.Content.Builder) is the command line tool for content processing.
* [KNIFXC](Tools/EffectCompiler) is the command line effect compiler tool.
* The [pipeline-editor](Tools/Content.Pipeline.Editor.WinForms) tool is a GUI frontend for content processing.

## Helpful Links

* [monogame.net](http://www.monogame.net).
* Our [issue tracker](https://github.com/kniengine/kni/issues) is on GitHub.
* Use [discussions forums](https://github.com/kniEngine/kni/discussions) for support questions.
* You can [join the Discord server](https://discord.gg/monogame) and chat live with the core developers and other users.
* The [MonoGame documentation](http://www.monogame.net/documentation/).
* Download release and development tools [Releases](https://github.com/kniEngine/kni/releases).

## License

The Kni project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code.  See the [LICENSE.txt](LICENSE.txt) file for more details.  Third-party libraries used by KNI are under their own licenses.  Please refer to those libraries for details on the license they use.

## Sponsors ❤️

While KNI is free and open-source, maintaining and expanding the framework requires ongoing effort and resources. We rely on the support of our community to continue delivering top-notch updates, features, and support.
By [becoming a Sponsor](https://github.com/sponsors/nkast), you can directly contribute to the growth and sustainability of the KNI Game Framework. 

<!-- sponsors --><a href="https://github.com/damian-666"><img src="https:&#x2F;&#x2F;github.com&#x2F;damian-666.png" width="60px" alt="User avatar: Damian" /></a><a href="https://github.com/MutsiMutsi"><img src="https:&#x2F;&#x2F;github.com&#x2F;MutsiMutsi.png" width="60px" alt="User avatar: Mitchel Disveld" /></a><a href="https://github.com/slyonics"><img src="https:&#x2F;&#x2F;github.com&#x2F;slyonics.png" width="60px" alt="User avatar: " /></a><!-- sponsors -->
