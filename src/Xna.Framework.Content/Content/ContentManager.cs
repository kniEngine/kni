// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;


namespace Microsoft.Xna.Framework.Content
{
    public class ContentManager : IDisposable
    {
        const byte ContentFlagCompressedLzx = 0x80;
        const byte ContentFlagCompressedLz4 = 0x40;
        const byte ContentFlagHiDef = 0x01;

        private readonly object SyncHandle = new object();

        private string _rootDirectory = string.Empty;
        private IServiceProvider _serviceProvider;
        private Dictionary<string, object> _loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private HashSet<IDisposable> _disposableAssets = new HashSet<IDisposable>();
        private bool _isDisposed;

        private static readonly List<char> _targetPlatformIdentifiers = new List<char>()
        {
            // XNA content identifiers
            'w', // Windows (XNA & DirectX)
            'x', // Xbox360 (XNA)
            'm', // WindowsPhone7.0 (XNA)

            // content identifiers
            'i', // iOS
            'a', // Android
            'd', // DesktopGL
            'X', // MacOSX
            'W', // WindowsStoreApp
            'n', // NativeClient
            'r', // RaspberryPi
            'P', // PlayStation4
            '5', // PlayStation5
            'O', // XboxOne
            'S', // Nintendo Switch
            'b', // BlazorGL

            // NOTE: There are additional idenfiers for consoles that 
            // are not defined in this repository.  Be sure to ask the
            // console port maintainers to ensure no collisions occur.

            
            // Legacy identifiers... these could be reused in the
            // future if we feel enough time has passed.

            'p', // PlayStationMobile
            'v', // PSVita
            'g', // Windows (OpenGL)
            'l', // Linux
            'M', // WindowsPhone8
            'G', // Google Stadia
        };

        public string RootDirectory
        {
            get { return _rootDirectory; }
            set { _rootDirectory = value; }
        }

        public IServiceProvider ServiceProvider
        {
            get { return this._serviceProvider; }
        }


        public ContentManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this._serviceProvider = serviceProvider;
        }

        public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");
            if (rootDirectory == null)
                throw new ArgumentNullException("rootDirectory");

            this.RootDirectory = rootDirectory;
            this._serviceProvider = serviceProvider;
        }



        public virtual T LoadLocalized<T> (string assetName)
        {
            string [] cultureNames =
            {
                CultureInfo.CurrentCulture.Name,                        // eg. "en-US"
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName     // eg. "en"
            };

            // Look first for a specialized language-country version of the asset,
            // then if that fails, loop back around to see if we can find one that
            // specifies just the language without the country part.
            foreach (string cultureName in cultureNames)
            {
                string localizedAssetName = assetName + '.' + cultureName;

                try
                {
                    return Load<T>(localizedAssetName);
                }
                catch (ContentLoadException) { /* ignore */ }
            }

            // If we didn't find any localized asset, fall back to the default name.
            return Load<T>(assetName);
        }

        public virtual T Load<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentNullException("assetName");
            if (_isDisposed)
                throw new ObjectDisposedException("ContentManager");

            T result = default(T);
            
            // On some platforms, name and slash direction matter.
            // We store the asset by a /-seperating key rather than how the
            // path to the file was passed to us to avoid
            // loading "content/asset1.xnb" and "content\\ASSET1.xnb" as if they were two 
            // different files. This matches stock XNA behavior.
            // The dictionary will ignore case differences
            string key = assetName.Replace('\\', '/');

            lock (this.SyncHandle)
            {
                // Check for a previously loaded asset first
                object asset = null;
                if (_loadedAssets.TryGetValue(key, out asset))
                {
                    if (asset is T)
                        return (T)asset;
                }
                else
                {
                    _loadedAssets[key] = null;
                }
            }

            // Load the asset.
            result = ReadAsset<T>(assetName, this.RecordDisposable);

            lock (this.SyncHandle)
            {
                _loadedAssets[key] = result;
            }

            return result;
        }
        
        protected virtual Stream OpenStream(string assetName)
        {
            try
            {
                string assetPath = Path.Combine(RootDirectory, assetName) + ".xnb";

                // This is primarily for editor support. 
                // Setting the RootDirectory to an absolute path is useful in editor
                // situations, but TitleContainer can ONLY be passed relative paths.                
                if (Path.IsPathRooted(assetPath))
                    return File.OpenRead(assetPath);
                
                return TitleContainer.OpenStream(assetPath);
            }
            catch (FileNotFoundException fileNotFound)
            {
                throw new ContentLoadException("The content file was not found.", fileNotFound);
            }
            catch (DirectoryNotFoundException directoryNotFound)
            {
                throw new ContentLoadException("The directory was not found.", directoryNotFound);
            }
            catch (Exception exception)
            {
                throw new ContentLoadException("Opening stream error.", exception);
            }
        }

        protected T ReadAsset<T>(string assetName, Action<IDisposable> recordDisposableObject)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentNullException("assetName");
            if (_isDisposed)
                throw new ObjectDisposedException("ContentManager");
                        
            string originalAssetName = assetName;

            // Try to load as XNB file
            Stream stream = OpenStream(assetName);
            using (BinaryReader xnbReader = new BinaryReader(stream))
            {
                using (ContentReader reader = GetContentReaderFromXnb(assetName, stream, xnbReader, recordDisposableObject))
                {
                    T result = reader.ReadAsset<T>();

                    if (result == null)
                        throw new ContentLoadException("Could not load " + originalAssetName + " asset!");

                    return result;
                }
            }
        }

        private ContentReader GetContentReaderFromXnb(string originalAssetName, Stream stream, BinaryReader xnbReader, Action<IDisposable> recordDisposableObject)
        {
            // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
            byte x = xnbReader.ReadByte();
            byte n = xnbReader.ReadByte();
            byte b = xnbReader.ReadByte();
            byte platform = xnbReader.ReadByte();

            if (x != 'X' || n != 'N' || b != 'B')
                throw new ContentLoadException("Asset does not appear to be a valid XNB file.");

            if (!_targetPlatformIdentifiers.Contains((char)platform))
                throw new ContentLoadException("Asset does not appear to target a known platform. Platform Identifier: '" + (char)platform+"'.");

            byte version = xnbReader.ReadByte();

            if (version != 5 && version != 4)
                throw new ContentLoadException("Invalid XNB version");

            byte flags = xnbReader.ReadByte();

            bool isCompressedLzx = (flags & ContentFlagCompressedLzx) == ContentFlagCompressedLzx;
            bool isCompressedLz4 = (flags & ContentFlagCompressedLz4) == ContentFlagCompressedLz4;

            bool isHiDef = (flags & ContentFlagHiDef) != 0;

            // The next int32 is the length of the XNB file
            int compressedFileSize = xnbReader.ReadInt32();

            Stream decompressedStream = null;
            if (isCompressedLzx
            ||  isCompressedLz4)
            {
                // Decompress the xnb

                int decompressedDataSize = xnbReader.ReadInt32();
                int compressedDataSize = compressedFileSize - 14;

                if (isCompressedLzx)
                {
                    // LzxDecoderStream require a seekable stream.
                    // Handle the case of Android's BufferedStream assets.
                    Stream compressedStream = stream;
                    if (stream is BufferedStream && !stream.CanSeek)
                    {
                        compressedStream = new MemoryStream(compressedDataSize);
                        stream.CopyTo(compressedStream);
                        compressedStream.Seek(0, SeekOrigin.Begin);
                    }

                    decompressedStream = new LzxDecoderStream(compressedStream, decompressedDataSize, compressedDataSize);
                }
                else if (isCompressedLz4)
                {
                    decompressedStream = new Lz4DecoderStream(stream);
                }
            }
            else // no compression
            {
                decompressedStream = stream;
            }

            ContentReader reader = new ContentReader(this, decompressedStream,
                                                     originalAssetName, version, compressedFileSize, recordDisposableObject);
            
            return reader;
        }

        internal void RecordDisposable(IDisposable disposable)
        {
            Debug.Assert(disposable != null, "The disposable is null.");

            lock (this.SyncHandle)
            {
                _disposableAssets.Add(disposable);
            }
        }

        public virtual void Unload()
        {
            lock (this.SyncHandle)
            {
                foreach (IDisposable disposable in _disposableAssets)
                {
                    if (disposable != null)
                        disposable.Dispose();
                }

                _disposableAssets.Clear();
                _loadedAssets.Clear();
            }
        }


        #region IDisposable Implementation
        ~ContentManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                Unload();
            }

            _isDisposed = true;
        }
        #endregion IDisposable Implementation
    }
}
