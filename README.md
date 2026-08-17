# Vertex.NET.Impeller

<p align="center">
  <img width="300" height="300" src="https://raw.githubusercontent.com/HexaEngine/HexaEngine/master/icon.png">
</p>

[![NuGet Version](https://img.shields.io/nuget/v/Vertex.NET.Impeller)](https://www.nuget.org/packages/Vertex.NET.Impeller)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Vertex.NET.Impeller)](https://www.nuget.org/packages/Vertex.NET.Impeller)
[![License](https://img.shields.io/github/license/Qianyiaz/Vertex.NET.Impeller)](LICENSE.md)

**Vertex.NET.Impeller** is a lightweight, minimal C# wrapper for the Impeller rendering library, providing a thin, 1:1
binding to Impeller's C API. It is designed for developers who require direct, low-level access to Impeller's
high-performance 2D/3D rendering capabilities from .NET, without any added abstraction or overhead.

## 🚀 Features

- **Modern-Targeting** – Supports .NET 8, .NET 9 and .NET 10.
- **Minimal & Transparent** – A direct 1:1 mapping to Impeller's C functions, preserving the original API semantics.
- **Cross-Platform** – Runs on all platforms supported by Impeller, including Windows, Linux, and macOS.
- **Pre-Built Native Binaries** – The NuGet package includes ready-to-use native libraries, eliminating the need for
  manual Impeller installation.
- **Performance-Oriented** – Zero overhead and minimal marshaling, making it suitable for real-time graphics
  applications.
- **Version** – Built against Impeller
  commit [9d83c362f88e3575fe277e2ba3f1a83ddc7d1597](https://github.com/flutter/flutter/commit/9d83c362f88e3575fe277e2ba3f1a83ddc7d1597)
  to ensure ABI compatibility.

## 📦 Installation

Add the package via NuGet:

```bash
dotnet add package Vertex.NET.Impeller
```

You can also browse the [package page on NuGet](https://www.nuget.org/packages/Vertex.NET.Impeller) for more details.

## 🔨 Usage

For complete working examples, check out
the [example projects](https://github.com/Qianyiaz/Vertex.NET.Impeller/tree/main/Example) in the repository.

## 📚 Documentation

For in-depth information about Impeller's functionality, refer to the
official [Impeller documentation](https://github.com/flutter/flutter/blob/main/engine/src/flutter/impeller/README.md).
The wrapper's API is intentionally kept identical to the native library, so all original guides and references apply.

## 🤝 Contributing

Contributions are welcome! If you encounter any issues or have suggestions for improvements, feel free to:

* Open an [issue](https://github.com/Qianyiaz/Vertex.NET.Impeller/issues)
* Submit a [pull request](https://github.com/Qianyiaz/Vertex.NET.Impeller/pulls)

## 📄 License

Vertex.NET.Impeller is released under the MIT License. See the [LICENSE](LICENSE.md) file for full details.

## 🙏 Credits

[Impeller](https://github.com/flutter/flutter/tree/master/engine/src/flutter/impeller) – This project is a wrapper around the Impeller rendering engine,
developed by [the Flutter team](https://github.com/flutter).

[AvaloniaUI/NImpeller](https://github.com/AvaloniaUI/NImpeller) – Thanks to the AvaloniaUI team for their excellent
implementation and design inspiration, which served as a valuable reference.

[HexaGen](https://github.com/HexaEngine/HexaGen) – The structure and approach are inspired by the excellent code
generator for generating bindings.