// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace Kni.Tests
{
	abstract class FixtureBase
	{
		private MockGame _game;

		protected MockGame Game
		{
			get { return _game; }
		}

		[SetUp]
		public virtual void SetUp()
		{
			Paths.SetStandardWorkingDirectory();
			_game = new MockGame();
		}

		[TearDown]
		public virtual void TearDown()
		{
			_game.Dispose();
			_game = null;
		}
	}
}
