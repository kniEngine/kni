// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Kni.Tests.Components;
using NUnit.Framework;

namespace Kni.Tests.Graphics
{
	[TestFixture]
	class VisualMiscellaneousTests : VisualTestFixtureBase
	{
		[Test]
		public void DrawOrder_falls_back_to_order_of_addition_to_Game()
		{
			Game.PreDrawWith += (sender, e) =>
			{
				Game.GraphicsDevice.Clear(Color.CornflowerBlue);
			};

			Game.Components.Add(new ImplicitDrawOrderComponent(Game));
			RunMultiFrameTest(captureCount: 5);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TexturedQuad_lighting(bool enableLighting)
		{
			Game.Components.Add(new TexturedQuadComponent(Game, enableLighting));
			RunSingleFrameTest();
		}

		[Test]
		public void SpaceshipModel()
		{
			Game.Components.Add(new SpaceshipModelDrawComponent(Game));
			RunMultiFrameTest(captureCount: 10, captureStride: 2);
		}
	}
}
