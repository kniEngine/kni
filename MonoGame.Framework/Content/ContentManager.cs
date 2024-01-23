// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Platform.Content.Utilities;


namespace Microsoft.Xna.Framework.Content
{
    public partial class ContentManager : IDisposable
    {
        const byte ContentFlagCompressedLzx = 0x80;
        const byte ContentFlagCompressedLz4 = 0x40;
        const byte ContentFlagHiDef = 0x01;

        private string _rootDirectory = string.Empty;
        private IServiceProvider serviceProvider;
        private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, object> loadedSharedResources = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private List<IDisposable> disposableAssets = new List<IDisposable>();
        private bool disposed;

        private static object ContentManagerLock = new object();
        private static List<WeakReference> ContentManagers = new List<WeakReference>();

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


        static partial void PlatformStaticInit();

        static ContentManager()
        {
            // Allow any per-platform static initialization to occur.
            PlatformStaticInit();
        }

        private static void AddContentManager(ContentManager contentManager)
        {
            lock (ContentManagerLock)
            {
                // Check if the list contains this content manager already. Also take
                // the opportunity to prune the list of any finalized content managers.
                bool contains = false;
                for (int i = ContentManagers.Count - 1; i >= 0; --i)
                {
                    WeakReference contentRef = ContentManagers[i];
                    if (ReferenceEquals(contentRef.Target, contentManager))
                        contains = true;
                    if (!contentRef.IsAlive)
                        ContentManagers.RemoveAt(i);
                }
                if (!contains)
                    ContentManagers.Add(new WeakReference(contentManager));
            }
        }

        private static void RemoveContentManager(ContentManager contentManager)
        {
            lock (ContentManagerLock)
            {
                // Check if the list contains this content manager and remove it. Also
                // take the opportunity to prune the list of any finalized content managers.
                for (int i = ContentManagers.Count - 1; i >= 0; --i)
                {
                    WeakReference contentRef = ContentManagers[i];
                    if (!contentRef.IsAlive || ReferenceEquals(contentRef.Target, contentManager))
                        ContentManagers.RemoveAt(i);
                }
            }
        }

        internal static void ReloadGraphicsContent()
        {
            lock (ContentManagerLock)
            {
                // Reload the graphic assets of each content manager. Also take the
                // opportunity to prune the list of any finalized content managers.
                for (int i = ContentManagers.Count - 1; i >= 0; --i)
                {
                    WeakReference contentRef = ContentManagers[i];
                    if (contentRef.IsAlive)
                    {
                        ContentManager contentManager = (ContentManager)contentRef.Target;
                        if (contentManager != null)
                            contentManager.ReloadGraphicsAssets();
                    }
                    else
                    {
                        ContentManagers.RemoveAt(i);
                    }
                }
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ContentManager()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public ContentManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            AddContentManager(this);
        }

        public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (rootDirectory == null)
            {
                throw new ArgumentNullException("rootDirectory");
            }
            this.RootDirectory = rootDirectory;
            this.serviceProvider = serviceProvider;
            AddContentManager(this);
        }

        public void Dispose()
        {
            Dispose(true);
            // Tell the garbage collector not to call the finalizer
            // since all the cleanup will already be done.
            GC.SuppressFinalize(this);
            // Once disposed, content manager wont be used again
            RemoveContentManager(this);
        }

        // If disposing is true, it was called explicitly and we should dispose managed objects.
        // If disposing is false, it was called by the finalizer and managed objects should not be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Unload();
                }

                disposed = true;
            }
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
                    return Load<T> (localizedAssetName);
                }
                catch (ContentLoadException) { }
            }

            // If we didn't find any localized asset, fall back to the default name.
            return Load<T> (assetName);
        }

        public virtual T Load<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }
            if (disposed)
            {
                throw new ObjectDisposedException("ContentManager");
            }

            T result = default(T);
            
            // On some platforms, name and slash direction matter.
            // We store the asset by a /-seperating key rather than how the
            // path to the file was passed to us to avoid
            // loading "content/asset1.xnb" and "content\\ASSET1.xnb" as if they were two 
            // different files. This matches stock XNA behavior.
            // The dictionary will ignore case differences
            string key = assetName.Replace('\\', '/');

            // Check for a previously loaded asset first
            object asset = null;
            if (loadedAssets.TryGetValue(key, out asset))
            {
                if (asset is T)
                {
                    return (T)asset;
                }
            }

            // Load the asset.
            result = ReadAsset<T>(assetName, null);

            loadedAssets[key] = result;
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
            {
                throw new ArgumentNullException("assetName");
            }
            if (disposed)
            {
                throw new ObjectDisposedException("ContentManager");
            }
                        
            string originalAssetName = assetName;
            object result = null;

            // Try to load as XNB file
            Stream stream = OpenStream(assetName);
            using (BinaryReader xnbReader = new BinaryReader(stream))
            {
                using (ContentReader reader = GetContentReaderFromXnb(assetName, stream, xnbReader, recordDisposableObject))
                {
                    result = reader.ReadAsset<T>();
                }
            }
            
            if (result == null)
                throw new ContentLoadException("Could not load " + originalAssetName + " asset!");

            return (T)result;
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

            bool isCompressedLzx = (flags & ContentFlagCompressedLzx) != 0;
            bool isCompressedLz4 = (flags & ContentFlagCompressedLz4) != 0;
            bool isHiDef = (flags & ContentFlagHiDef) != 0;

            // The next int32 is the length of the XNB file
            int xnbLength = xnbReader.ReadInt32();

            Stream decompressedStream = null;
            if (isCompressedLzx || isCompressedLz4)
            {
                // Decompress the xnb
                int decompressedSize = xnbReader.ReadInt32();

                if (isCompressedLzx)
                {
                    int compressedSize = xnbLength - 14;
                    decompressedStream = new LzxDecoderStream(stream, decompressedSize, compressedSize);
                }
                else if (isCompressedLz4)
                {
                    decompressedStream = new Lz4DecoderStream(stream);
                }
            }
            else
            {
                decompressedStream = stream;
            }

            ContentReader reader = new ContentReader(this, decompressedStream,
                                                     originalAssetName, version, xnbLength, recordDisposableObject);
            
            return reader;
        }

        internal void RecordDisposable(IDisposable disposable)
        {
            Debug.Assert(disposable != null, "The disposable is null!");

            // Avoid recording disposable objects twice. ReloadAsset will try to record the disposables again.
            // We don't know which asset recorded which disposable so just guard against storing multiple of the same instance.
            if (!disposableAssets.Contains(disposable))
                disposableAssets.Add(disposable);
        }

        /// <summary>
        /// Virtual property to allow a derived ContentManager to have it's assets reloaded
        /// </summary>
        protected virtual Dictionary<string, object> LoadedAssets
        {
            get { return loadedAssets; }
        }

        protected virtual void ReloadGraphicsAssets()
        {
            foreach (KeyValuePair<string,object> asset in LoadedAssets)
            {
                // This never executes as asset.Key is never null.  This just forces the 
                // linker to include the ReloadAsset function when AOT compiled.
                if (asset.Key == null)
                    ReloadAsset(asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()));

                MethodInfo methodInfo = ReflectionHelpers.GetMethodInfo(typeof(ContentManager), "ReloadAsset");
                MethodInfo genericMethod = methodInfo.MakeGenericMethod(asset.Value.GetType());
                genericMethod.Invoke(this, new object[] { asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()) }); 
            }
        }

        protected virtual void ReloadAsset<T>(string originalAssetName, T currentAsset)
        {
            string assetName = originalAssetName;
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }
            if (disposed)
            {
                throw new ObjectDisposedException("ContentManager");
            }

            Stream stream = OpenStream(assetName);
            using (BinaryReader xnbReader = new BinaryReader(stream))
            {
                using (ContentReader reader = GetContentReaderFromXnb(assetName, stream, xnbReader, null))
                {
                    reader.ReadAsset<T>(currentAsset);
                }
            }
        }

        public virtual void Unload()
        {
            // Look for disposable assets.
            foreach (IDisposable disposable in disposableAssets)
            {
                if (disposable != null)
                    disposable.Dispose();
            }

            disposableAssets.Clear();
            loadedAssets.Clear();
            loadedSharedResources.Clear();
        }

        public string RootDirectory
        {
            get { return _rootDirectory; }
            set { _rootDirectory = value; }
        }
        
        public IServiceProvider ServiceProvider
        {
            get { return this.serviceProvider; }
        }

    }
}
