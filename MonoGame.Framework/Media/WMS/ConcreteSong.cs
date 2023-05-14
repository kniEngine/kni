// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using SharpDX;
using SharpDX.MediaFoundation;
using MediaFoundation = SharpDX.MediaFoundation;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteSongStrategy : SongStrategy
    {
        private Uri _streamSource;
        private Topology _topology;

        internal Uri StreamSource { get { return _streamSource; } }
        internal Topology Topology { get { return _topology; } }


        internal ConcreteSongStrategy(string name, Uri streamSource)
        {
            this.Name = name;
            this._streamSource = streamSource;

            MediaManager.Startup(true);
            this._topology = CreateTopology(streamSource);
        }

        private Topology CreateTopology(Uri streamSource)
        {
            Topology topology;
            MediaFoundation.MediaFactory.CreateTopology(out topology);

            SharpDX.MediaFoundation.MediaSource mediaSource;
            {
                string filename = streamSource.OriginalString;

                SourceResolver resolver = new SourceResolver();
                ComObject source = resolver.CreateObjectFromURL(filename, SourceResolverFlags.MediaSource);
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
                    MediaFoundation.MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

                    sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
                    sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                    sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                    TopologyNode outputNode;
                    MediaFoundation.MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

                    var typeHandler = desc.MediaTypeHandler;
                    var majorType = typeHandler.MajorType;
                    if (majorType != MediaTypeGuids.Audio)
                        throw new NotSupportedException("The song contains video data!");

                    Activate activate;
                    MediaFoundation.MediaFactory.CreateAudioRendererActivate(out activate);
                    outputNode.Object = activate;

                    topology.AddNode(sourceNode);
                    topology.AddNode(outputNode);
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

            return topology;
        }

        public override Album Album
        {
            get { return null; }
        }

        public override Artist Artist
        {
            get { return null; }
        }

        public override Genre Genre
        {
            get { return null; }
        }

        public override TimeSpan Duration
        {
            get { return base.Duration; }
        }

        public override bool IsProtected
        {
            get { return base.IsProtected; }
        }

        public override bool IsRated
        {
            get { return base.IsRated; }
        }

        internal override string Filename
        {
            get { return StreamSource.OriginalString; }
        }

        public override string Name
        {
            get { return base.Name; }
        }

        public override int PlayCount
        {
            get { return base.PlayCount; }
        }

        public override int Rating
        {
            get { return base.Rating; }
        }

        public override int TrackNumber
        {
            get { return base.TrackNumber; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_topology != null)
                {
                    _topology.Dispose();
                    _topology = null;
                }
            }

            MediaManager.Shutdown();

            //base.Dispose(disposing);

        }
    }
}

