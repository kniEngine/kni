﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;

namespace Kni.Tests.Framework
{
    class FrameworkDispatcherTest
    {
        [Test]
        public void CallOnPrimaryThread()
        {
            FrameworkDispatcher.Update();
        }

        [Test]
        public void CallOnAnotherThread()
        {
            // Ensure that FrameworkDispatcher is initialized on the main thread.
            FrameworkDispatcher.Update();

            _callOnAnotherThreadResult = CallOnAnotherThreadTestResult.NotRun;

            var thread = new Thread(() => {
                _callOnAnotherThreadResult = CallOnAnotherThreadTestResult.Exception;
                FrameworkDispatcher.Update();

                // If executing this line, no exception was thrown.
                _callOnAnotherThreadResult = CallOnAnotherThreadTestResult.NoException;

            });

            thread.Start();
            if (!thread.Join(1000))
                Assert.Fail("Secondary thread did not terminate in time.");

            Assert.AreEqual(CallOnAnotherThreadTestResult.NoException, _callOnAnotherThreadResult);
        }
        private static CallOnAnotherThreadTestResult _callOnAnotherThreadResult;

        enum CallOnAnotherThreadTestResult
        {
            NotRun,
            NoException,
            Exception
        }

#if !XNA
        [Test]
        public void UpdatesSoundEffectInstancePool()
        {
            FrameworkDispatcher.Update();
            var sfx = new SoundEffect(new byte[] { 0, 0 }, 44100, AudioChannels.Mono);

            sfx.Play();
            Assert.AreEqual(1, GetPlayingSoundCount());
            Thread.Sleep(25); // Give the sound effect time to play

            FrameworkDispatcher.Update();
            Assert.AreEqual(0, GetPlayingSoundCount());
        }

        private int GetPlayingSoundCount()
        {
            // SoundEffectInstancePool._playingInstances is private
            // and not worth making internal only for this test.
            // Use reflection to get it.
            var fieldInfo = typeof(Microsoft.Xna.Platform.Audio.AudioService).GetField("_playingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
            var field = (LinkedList<SoundEffectInstance>)fieldInfo.GetValue(Microsoft.Xna.Platform.Audio.AudioService.Current);

            return field.Count;
        }
#endif
    }
}
