// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using SharpDX;
using SharpDX.MediaFoundation;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        private Topology _topology;

        internal Topology Topology { get { return _topology; } }

        internal override void PlatformInitialize(string fileName)
        {
            if (_topology != null)
                return;

            MediaManagerState.CheckStartup();

            MediaFactory.CreateTopology(out _topology);

            SharpDX.MediaFoundation.MediaSource mediaSource;
            {
                SourceResolver resolver = new SourceResolver();

                ComObject source = resolver.CreateObjectFromURL(FilePath, SourceResolverFlags.MediaSource);
                mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
                resolver.Dispose();
                source.Dispose();
            }

            PresentationDescriptor presDesc;
            mediaSource.CreatePresentationDescriptor(out presDesc);

            for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
            {
                SharpDX.Mathematics.Interop.RawBool selected;
                StreamDescriptor desc;
                presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);

                if (selected)
                {
                    TopologyNode sourceNode;
                    MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

                    sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
                    sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                    sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                    TopologyNode outputNode;
                    MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

                    var typeHandler = desc.MediaTypeHandler;
                    var majorType = typeHandler.MajorType;
                    if (majorType != MediaTypeGuids.Audio)
                        throw new NotSupportedException("The song contains video data!");

                    Activate activate;
                    MediaFactory.CreateAudioRendererActivate(out activate);
                    outputNode.Object = activate;

                    _topology.AddNode(sourceNode);
                    _topology.AddNode(outputNode);
                    sourceNode.ConnectOutput(0, outputNode, 0);

                    sourceNode.Dispose();
                    outputNode.Dispose();
                    typeHandler.Dispose();
                    activate.Dispose();
                }

                desc.Dispose();
            }

            presDesc.Dispose();
            mediaSource.Dispose();
        }

        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                if (_topology != null)
                {
                    _topology.Dispose();
                    _topology = null;
                }
            }
        }

        internal override Album PlatformGetAlbum()
        {
            return null;
        }

        internal override void PlatformSetAlbum(Album album)
        {
            
        }

        internal override Artist PlatformGetArtist()
        {
            return null;
        }

        internal override Genre PlatformGetGenre()
        {
            return null;
        }

        internal override TimeSpan PlatformGetDuration()
        {
            return _duration;
        }

        internal override bool PlatformIsProtected()
        {
            return false;
        }

        internal override bool PlatformIsRated()
        {
            return false;
        }

        internal override string PlatformGetName()
        {
            return Path.GetFileNameWithoutExtension(_name);
        }

        internal override int PlatformGetPlayCount()
        {
            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}

