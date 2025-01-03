// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace Kni.Tests
{
	static partial class GameTest
	{

		[TestFixture]
		public class Disposal : FixtureBase
		{
			[TestCase("Components")]
			[TestCase("Content")]
			[TestCase("GraphicsDevice")]
			[TestCase("InactiveSleepTime")]
			[TestCase("IsActive")]
			[TestCase("IsFixedTimeStep")]
			[TestCase("IsMouseVisible")]
			[TestCase("LaunchParameters")]
			[TestCase("Services")]
			[TestCase("TargetElapsedTime")]
			[TestCase("Window")]
			public void Property_does_not_throws_after_Dispose(string propertyName)
			{
				var propertyInfo = Game.GetType().GetProperty(propertyName);
				if (propertyInfo == null)
					Assert.Fail("Property '{0}' not found", propertyName);

				Game.Dispose();
				AssertDoesNotThrow<ObjectDisposedException>(() =>
					RunAndUnpackException(() => propertyInfo.GetValue(Game, null)));
			}

			[TestCase("Dispose")]
			[TestCase("Exit")]
			[TestCase("ResetElapsedTime")]
			[TestCase("Run")]
			[TestCase("RunOneFrame")]
			[TestCase("SuppressDraw")]
			[TestCase("Tick")]
			public void Method_does_not_throw_after_Dispose(string methodName)
			{
				var methodInfo = Game.GetType().GetMethod(methodName, new Type[0]);
				if (methodInfo == null)
					Assert.Fail("Method '{0}' not found", methodName);

				Game.Dispose();
				AssertDoesNotThrow<ObjectDisposedException>(() =>
					RunAndUnpackException(() => methodInfo.Invoke(Game, null)));
			}

			private void RunAndDispose()
			{
				Game.MakeGraphical();
				Game.Run();
				Game.Dispose();
			}

			private static void RunAndUnpackException(Action action)
			{
				try
				{
					action();
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException;
				}
			}

			private void RunDisposeAndTry(Action action, string name, bool shouldThrow)
			{
				RunAndDispose();
				bool didThrow = false;
				try
				{
					action();
				}
				catch (ObjectDisposedException)
				{
					if (!shouldThrow)
						Assert.Fail("{0} threw ObjectDisposed when it shouldn't have.", name);
					didThrow = true;
				}
				catch (TargetInvocationException ex)
				{
					if (!(ex.InnerException is ObjectDisposedException))
						throw;

					if (!shouldThrow)
						Assert.Fail("{0} threw ObjectDisposed when it shouldn't have.", name);
					didThrow = true;
				}
				if (didThrow && !shouldThrow)
					Assert.Fail("{0} did not throw ObjectDisposedException when it should have.",
							 name);
			}

			private static void AssertDoesNotThrow<T>(TestDelegate code) where T : Exception
			{
				try
				{
					code();
				}
				catch (T ex)
				{
					Assert.AreEqual(null, ex);
				}
				catch (Exception ex)
				{
					Console.WriteLine(
						"AssertDoesNotThrow<{0}> caught and ignored {1}", typeof(T), ex);
				}
			}
		}

		[TestFixture]
		public class Behaviors : FixtureBase
		{
			[Test]
			public void Nongraphical_run_succeeds()
			{
				Game.Run();

				Assert.That(Game, Has.Property("UpdateCount").EqualTo(1));
				Assert.That(Game, Has.Property("DrawCount").EqualTo(0));
			}

			[Test, Ignore("Fix me!")]
			public void Fixed_time_step_skips_draw_when_update_is_slow()
			{
				Game.MakeGraphical();

				var targetElapsedTime = TimeSpan.FromSeconds(1f / 10f);
				var slowUpdateTime = TimeSpan.FromSeconds(targetElapsedTime.TotalSeconds * 2);

				var slowUpdater = new SlowUpdater(Game, slowUpdateTime);
				Game.Components.Add(slowUpdater);

				var logger = new RunLoopLogger(Game);
				Game.Components.Add(logger);

				Game.MaxUpdateCount = int.MaxValue;
				Game.MaxDrawCount = 100;

				Game.IsFixedTimeStep = true;
				Game.TargetElapsedTime = targetElapsedTime;
				Game.Run();

				//Assert.That(_game, Has.Property("UpdateCount").GreaterThan(11));
				//Assert.That(_game, Has.Property("DrawCount").EqualTo(10));
			}

			[Test]
			public void GameTickTest()
			{
				// should not throw an exception
				Game.ResetElapsedTime();
				Assert.DoesNotThrow(() => Game.Tick());
				Game.ResetElapsedTime();
			}
		}

		[TestFixture]
		public class Misc
		{
			[Test]
			public void LoadContentNotCalledWithoutGdm()
			{
				var g = new CountCallsGame();
				g.PublicInitialize();

				Assert.AreEqual(0, g.LoadContentCount);

				g.Dispose();
			}

			[Test]
			public void LoadContentNotCalledWithoutGd()
			{
				var g = new CountCallsGame();
				var gdm = new GraphicsDeviceManager(g);

				g.PublicInitialize();

				Assert.AreEqual(0, g.LoadContentCount);

				g.Dispose();
			}

			[Test]
			public void ExitHappensAtEndOfTick()
			{
				// Exit called in Run
				var g = new ExitTestGame();

				// TODO this is not necessary for XNA, but MG crashes when no GDM is set and Run is called
				new GraphicsDeviceManager(g);
				g.Run();
				Assert.AreEqual(2, g.UpdateCount);
				Assert.AreEqual(0, g.DrawCount); // Draw should be suppressed
				Assert.AreEqual(1, g.ExitingCount);

				g.Dispose();
			}



			private class ExitTestGame : CountCallsGame
			{
				private int count = 0;

				protected override void Update(GameTime gameTime)
				{
					if (count > 0)
						Exit();

					base.Update(gameTime);
					Assert.IsNotNull(Window);
					Assert.AreEqual(0, ExitingCount);

					count++;
				}
			}

		}

		private class SlowUpdater : GameComponent
		{
			private TimeSpan _updateTime;
			public SlowUpdater(Game game, TimeSpan updateTime) :
				base(game)
			{
				_updateTime = updateTime;
			}

			int _count = 0;
			public override void Update(GameTime gameTime)
			{
				base.Update(gameTime);

				if (_count >= 4)
					return;

				_count++;

				//if (!gameTime.IsRunningSlowly)
				{
					var endTick = Stopwatch.GetTimestamp() +
							  (long)(Stopwatch.Frequency * _updateTime.TotalSeconds);
					//long endTick = (long)(_updateTime.TotalMilliseconds * 10) + DateTime.Now.Ticks;
					while (Stopwatch.GetTimestamp() < endTick)
					{
						// Be busy!
					}
				}
			}
		}

		private class RunLoopLogger : DrawableGameComponent
		{
			public RunLoopLogger(Game game) :
				base(game)
			{
			}

			private List<Entry> _entries = new List<Entry>();
			public IEnumerable<Entry> GetEntries()
			{
				return _entries.ToArray();
			}

			public override void Update(GameTime gameTime)
			{
				base.Update(gameTime);
				_entries.Add(Entry.FromUpdate(gameTime));
			}

			public override void Draw(GameTime gameTime)
			{
				base.Draw(gameTime);
				_entries.Add(Entry.FromDraw(gameTime));
			}

			public string GetLogString()
			{
				return string.Join(" ", _entries);
			}

			public struct Entry
            {
                public RunLoopAction Action { get; set; }
                public TimeSpan ElapsedGameTime { get; set; }
                public TimeSpan TotalGameTime { get; set; }
                public bool WasRunningSlowly { get; set; }

                public static Entry FromDraw(GameTime gameTime)
				{
					return new Entry
					{
						Action = RunLoopAction.Draw,
						ElapsedGameTime = gameTime.ElapsedGameTime,
						TotalGameTime = gameTime.TotalGameTime,
						WasRunningSlowly = gameTime.IsRunningSlowly
					};
				}

				public static Entry FromUpdate(GameTime gameTime)
				{
					return new Entry
					{
						Action = RunLoopAction.Update,
						ElapsedGameTime = gameTime.ElapsedGameTime,
						TotalGameTime = gameTime.TotalGameTime,
						WasRunningSlowly = gameTime.IsRunningSlowly
					};
				}

				public override string ToString()
				{
					char actionInitial;
					switch (Action)
					{
						case RunLoopAction.Draw: actionInitial = 'd'; break;
						case RunLoopAction.Update: actionInitial = 'u'; break;
						default: throw new NotSupportedException(Action.ToString());
					}

					return string.Format(
							   "{0}({1:0}{2})",
							   actionInitial,
							   ElapsedGameTime.TotalMilliseconds,
							   WasRunningSlowly ? "!" : "");
				}
			}
		}

		private enum RunLoopAction
		{
			Draw,
			Update
		}
	}
}
