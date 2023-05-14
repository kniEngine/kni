// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using MediaFoundation = SharpDX.MediaFoundation;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoStrategy : VideoStrategy
    {
        private Topology _topology;
        private VideoSampleGrabber _sampleGrabber;
        MediaType _mediaType;

        internal Topology Topology { get { return _topology; } }
        internal VideoSampleGrabber SampleGrabber { get { return _sampleGrabber; } }


        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {
            MediaManager.Startup(true);

            MediaFoundation.MediaFactory.CreateTopology(out _topology);

            MediaSource mediaSource;
            {
                SourceResolver resolver = new SourceResolver();

                ObjectType otype;
                ComObject source = resolver.CreateObjectFromURL(FileName, SourceResolverFlags.MediaSource, null, out otype);
                mediaSource = source.QueryInterface<MediaSource>();
                resolver.Dispose();
                source.Dispose();
            }

            PresentationDescriptor presDesc;
            mediaSource.CreatePresentationDescriptor(out presDesc);

            for (int i = 0; i < presDesc.StreamDescriptorCount; i++)
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

                    Guid majorType = desc.MediaTypeHandler.MajorType;
                    if (majorType == MediaTypeGuids.Video)
                    {
                        _mediaType = new MediaType();
                        _mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                        // Specify that we want the data to come in as RGB32.
                        _mediaType.Set(MediaTypeAttributeKeys.Subtype, new Guid("00000016-0000-0010-8000-00AA00389B71"));

                        _sampleGrabber = new VideoSampleGrabber();
                        Activate activate;
                        MediaFoundation.MediaFactory.CreateSampleGrabberSinkActivate(_mediaType, _sampleGrabber, out activate);
                        outputNode.Object = activate;
                    }

                    if (majorType == MediaTypeGuids.Audio)
                    {
                        Activate activate;
                        MediaFoundation.MediaFactory.CreateAudioRendererActivate(out activate);
                        outputNode.Object = activate;
                    }

                    _topology.AddNode(sourceNode);
                    _topology.AddNode(outputNode);
                    sourceNode.ConnectOutput(0, outputNode, 0);

                    sourceNode.Dispose();
                    outputNode.Dispose();
                }

                desc.Dispose();
            }

            presDesc.Dispose();
            mediaSource.Dispose();
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

                if (_sampleGrabber != null)
                {
                    _sampleGrabber.Dispose();
                    _sampleGrabber = null;
                }

            }

            MediaManager.Shutdown();

            base.Dispose(disposing);
        }
    }
}
