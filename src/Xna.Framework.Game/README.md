# Xna.Framework.Game

Game loop, window management, and component framework for KNI games.

## Overview

The `Xna.Framework.Game` package provides the core game loop and window management abstraction. It includes:

- **Game Class**: Main entry point with Update/Draw loop
- **GameWindow**: Platform-agnostic window management with fullscreen/windowed support
- **GameTime**: Frame timing with elapsed time and total game time
- **GameComponent System**: Updateable and drawable component architecture
- **GraphicsDeviceManager**: Automatic graphics setup and reset handling
- **GameServiceContainer**: Dependency injection for game services

## Installation

```bash
dotnet add package nkast.Xna.Framework.Game
```

## Quick Start

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class MyGame : Game
{
	private GraphicsDeviceManager graphics;
	private SpriteBatch spriteBatch;

	public MyGame()
	{
		Content.RootDirectory = "Content";
		graphics = new GraphicsDeviceManager(this);
		graphics.PreferredBackBufferWidth = 1280;
		graphics.PreferredBackBufferHeight = 720;
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);
	}

	protected override void Update(GameTime gameTime)
	{
		// Update game logic
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.SetRenderTarget(null);
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		// Draw game content
		spriteBatch.End();
	}
}

class Program
{
	static void Main()
	{
		using (Game game = new MyGame())
			game.Run();
	}
}
```
