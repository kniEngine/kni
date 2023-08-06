﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDebug : GraphicsDebugStrategy
    {
        private readonly D3D11.InfoQueue _infoQueue;
        private readonly Queue<GraphicsDebugMessage> _cachedMessages;
        private bool _hasPushedFilters = false;


        internal ConcreteGraphicsDebug(GraphicsDevice device)
            : base(device)
        {
            GraphicsContext context = device.CurrentContext;
            D3D11.DeviceContext d3DContext =((ConcreteGraphicsContext)context.Strategy).D3dContext;

            _infoQueue = d3DContext.QueryInterfaceOrNull<D3D11.InfoQueue>();
            _cachedMessages = new Queue<GraphicsDebugMessage>();

            if (_infoQueue != null)
            {
                _infoQueue.PushEmptyRetrievalFilter();
                _infoQueue.PushEmptyStorageFilter();
                _hasPushedFilters = true;
            }
        }


        public override bool TryDequeueMessage(out GraphicsDebugMessage message)
        {
            if (_infoQueue == null)
            {
                message = null;
                return false;
            }

            if (!_hasPushedFilters)
            {
                _infoQueue.PushEmptyRetrievalFilter();
                _infoQueue.PushEmptyStorageFilter();
                _hasPushedFilters = true;
            }

            if (_cachedMessages.Count > 0)
            {
                message = _cachedMessages.Dequeue();
                return true;
            }

            if (_infoQueue.NumStoredMessagesAllowedByRetrievalFilter > 0)
            {
                // Grab all current messages and put them in the cached messages queue.
                for (var i = 0; i < _infoQueue.NumStoredMessagesAllowedByRetrievalFilter; i++)
                {
                    var dxMessage = _infoQueue.GetMessage(i);
                    _cachedMessages.Enqueue(new GraphicsDebugMessage
                    {
                        Message = dxMessage.Description,
                        Id = (int)dxMessage.Id,
                        IdName = dxMessage.Id.ToString(),
                        Severity = dxMessage.Severity.ToString(),
                        Category = dxMessage.Category.ToString()
                    });
                }

                _infoQueue.ClearStoredMessages();
            }
            
            if (_cachedMessages.Count > 0)
            {
                message = _cachedMessages.Dequeue();
                return true;
            }
            
            // No messages to grab from DirectX.
            message = null;
            return false;
        }
    }
}
