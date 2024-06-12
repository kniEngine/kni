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
	class MockGame : TestGameBase
	{
		public int MinUpdateCount { get; set; }
		public int MaxUpdateCount { get; set; }
		public int MinDrawCount { get; set; }
		public int MaxDrawCount { get; set; }
		public int UpdateCount { get; private set; }
		public int DrawCount { get; private set; }
		public ExitReason ExitReason { get; private set; }

		public MockGame()
		{
			MinUpdateCount = int.MaxValue;
			MinDrawCount = int.MaxValue;
			MaxUpdateCount = 1;
			MaxDrawCount = 1;
		}

		public MockGame MakeGraphical()
		{
			if (Services.GetService(typeof(IGraphicsDeviceManager)) == null)
				new GraphicsDeviceManager(this);
			return this;
		}

		private void EvaluateExitCriteria()
		{
			ExitReason reason;
			if (UpdateCount >= MinUpdateCount && DrawCount >= MinDrawCount)
				reason = ExitReason.MinimumsSatisfied;
			else if (UpdateCount >= MaxUpdateCount)
				reason = ExitReason.MaxUpdateSatisfied;
			else if (DrawCount >= MaxDrawCount)
				reason = ExitReason.MaxDrawSatisfied;
			else
				reason = ExitReason.None;

			if (reason != ExitReason.None)
			{
				ExitReason = reason;
				DoExit();
			}
		}

		protected override void BeginRun()
		{
			base.BeginRun();
			UpdateCount = 0;
			DrawCount = 0;
		}

		protected override void EndRun()
		{
			base.EndRun();
#if XNA
                AbsorbQuitMessage();
#endif
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			UpdateCount++;
			EvaluateExitCriteria();
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			DrawCount++;
			EvaluateExitCriteria();
		}

	}

	public enum ExitReason
	{
		None,
		MinimumsSatisfied,
		MaxUpdateSatisfied,
		MaxDrawSatisfied
	}

}
