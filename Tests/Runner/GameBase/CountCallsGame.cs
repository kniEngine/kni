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
	class CountCallsGame : Game
	{
		public int BeginRunCount { get; set; }
		public int InitializeCount { get; set; }
		public int LoadContentCount { get; set; }
		public int UnloadContentCount { get; set; }
		public int UpdateCount { get; set; }
		public int BeginDrawCount { get; set; }
		public int DrawCount { get; set; }
		public int EndDrawCount { get; set; }
		public int EndRunCount { get; set; }
		public int DeactivatedCount { get; set; }
		public int ActivatedCount { get; set; }
		public int ExitingCount { get; set; }
		public int DisposeCount { get; set; }

		public void PublicBeginRun() { BeginRun(); }
		protected override void BeginRun() { BeginRunCount++; base.BeginRun(); }
		public void PublicInitialize() { Initialize(); }
		protected override void Initialize() { InitializeCount++; base.Initialize(); }
		public void PublicLoadContent() { LoadContent(); }
		protected override void LoadContent() { LoadContentCount++; base.LoadContent(); }
		public void PublicUnloadContent() { UnloadContent(); }
		protected override void UnloadContent() { UnloadContentCount++; base.UnloadContent(); }
		public void PublicUpdate(GameTime gt = null) { Update(gt ?? new GameTime()); }
		protected override void Update(GameTime gameTime) { UpdateCount++; base.Update(gameTime); }
		public bool PublicBeginDraw() { return BeginDraw(); }
		protected override bool BeginDraw() { BeginDrawCount++; return base.BeginDraw(); }
		public void PublicDraw(GameTime gt) { Draw(gt ?? new GameTime()); }
		protected override void Draw(GameTime gameTime) { DrawCount++; base.Draw(gameTime); }
		public bool PublicEndDraw() { return BeginDraw(); }
		protected override void EndDraw() { EndDrawCount++; base.EndDraw(); }
		public void PublicEndRun() { EndRun(); }
		protected override void EndRun() { EndRunCount++; base.EndRun(); }

#if XNA
        protected override void OnActivated(object sender, EventArgs args) { ActivatedCount++; base.OnActivated(sender, args); }
        protected override void OnDeactivated(object sender, EventArgs args) { DeactivatedCount++; base.OnDeactivated(sender, args); }
        protected override void OnExiting(object sender, EventArgs args) { ExitingCount++; base.OnExiting(sender, args); }
#else
		protected override void OnActivated(EventArgs args) { ActivatedCount++; base.OnActivated(args); }
		protected override void OnDeactivated(EventArgs args) { DeactivatedCount++; base.OnDeactivated(args); }
		protected override void OnExiting(EventArgs args) { ExitingCount++; base.OnExiting(args); }
#endif
		protected override void Dispose(bool disposing) { DisposeCount++; base.Dispose(disposing); }
	}
}
