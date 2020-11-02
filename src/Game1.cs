using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using Num = System.Numerics;

namespace MGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer _imGuiRenderer;
        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

        private Texture2D pixel;

        private SpriteBatch _spriteBatch;

        private static Rectangle CANVAS = new Rectangle(0, 0, 1920, 1080);
        private static float SCALE = 172f;
        private static float spriteScale = 1f;
        private RenderTarget2D _finalGameTarget;

        private float zNear, zFar;
        private float speed = 2f;
        private Camera camera;
        //private Matrix projectionMatrix;
        private Matrix worldToScreenSpaceMatrix;
        //private Matrix viewProjectionMatrix;

        private Grid grid;

        private Texture2D tile;
        private Vector3[] tiles;
        private Vector3[] transformedTiles;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here}
            _graphics.PreferredBackBufferWidth = CANVAS.Width;
            _graphics.PreferredBackBufferHeight = CANVAS.Height;
            _graphics.IsFullScreen = false;
            _graphics.PreferMultiSampling = true;
            _graphics.SynchronizeWithVerticalRetrace = true;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();

            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();
            
            zNear = -100; zFar = 100;
            camera = new Camera(CANVAS.Width, CANVAS.Height, zNear, zFar);
            camera.Position.X = 0.05f;
            camera.Position.Y = -0.05f;
            camera.Position.Z = 0.05f;
            camera.Rotation.X = 54.736f+180f; // Figure out later
            camera.Rotation.Y = 0f;
            camera.Rotation.Z = 45f;
            camera.Zoom = 1.0f;
            worldToScreenSpaceMatrix = new Matrix();
            //viewProjectionMatrix = new Matrix();

            grid = new Grid(10);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _finalGameTarget = new RenderTarget2D(GraphicsDevice, CANVAS.Width, CANVAS.Height);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
			_xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
			{
				var red = (pixel % 300) / 2;
				return new Color(red, 1, 1);
			});

			// Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls (see below)
			_imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);

            // Initialize UI Pixel
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            Color[] pixelColor = new Color[1];
            pixelColor[0] = Color.White;
            pixel.SetData(pixelColor);

            tile = Content.Load<Texture2D>(@"textures/tile");
            tiles = new Vector3[4];
            tiles[0] = new Vector3();
            tiles[1] = new Vector3(1f, 1f, 0f);
            tiles[2] = new Vector3(2f, 0f, 0f);
            tiles[3] = new Vector3(4f, 0f, 0f);
            transformedTiles = new Vector3[4];
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            camera.DeriveWorldToScreenSpaceTransformationMatrix(SCALE, out worldToScreenSpaceMatrix);
            ref Matrix viewMatrix = ref camera.GetViewRotationMatrix();
            Matrix inverseRotationMatrix = Matrix.Invert(viewMatrix);

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            Vector3 controllerVector = new Vector3(
                gamePadState.ThumbSticks.Right.X,
                -gamePadState.ThumbSticks.Right.Y*2f,
                0f
            );

            Vector3 viewAlignedControllerAxis = Vector3.Transform(controllerVector, inverseRotationMatrix);

            camera.Position.X += viewAlignedControllerAxis.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            camera.Position.Y += viewAlignedControllerAxis.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //tilePos.X += 0.1f;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_finalGameTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();
            grid.Draw(ref worldToScreenSpaceMatrix, _spriteBatch, pixel);

            //Vector3.Transform(ref tilePos, ref viewProjectionMatrix, out transformedTilePos);
            Vector3.Transform(tiles, ref worldToScreenSpaceMatrix, transformedTiles);
            // Sort by Y before Drawing
            Array.Sort(transformedTiles, (t1, t2) => t1.Y.CompareTo(t2.Y));

            for(int i=0; i<transformedTiles.Length; i++) {
                Vector3 t = transformedTiles[i];
                _spriteBatch.Draw(
                    texture: tile,
                    position: new Vector2(t.X, t.Y),
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0f,
                    origin: new Vector2(tile.Width/2f, tile.Height/2f),
                    scale: 1/camera.Zoom*spriteScale,
                    effects: SpriteEffects.None,
                    layerDepth: 0f
                );
            }
            
            _spriteBatch.End(); 

            /** Render final render target to screen */
            GraphicsDevice.SetRenderTarget(null);

             _spriteBatch.Begin();
             _spriteBatch.Draw(_finalGameTarget, new Rectangle(0,0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
             _spriteBatch.End();
            
            _imGuiRenderer.BeforeLayout(gameTime);
            ImGuiLayout();
            _imGuiRenderer.AfterLayout();
            
            base.Draw(gameTime);
        }

        private void SortSprites(ref Vector3[] input) {
            for(int i=0; i<input.Length-1; i++) {
                Vector3 iVector = input[i];
                int min = i;
                float minSum = iVector.Z;
                for(int j=i; j<input.Length-1; j++) {
                    Vector3 jV = input[j];
                    float jSum = jV.Z;
                    if(jSum < minSum) {
                        min = j;
                        minSum = jSum;
                    }
                }
                Vector3 temp = input[i];
                input[i] = input[min];
                input[min] = temp;
            }
        }

        private bool show_test_window = false;
        private bool show_another_window = false;
        private Num.Vector3 clear_color = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);
        private byte[] _textBuffer = new byte[100];


        protected virtual void ImGuiLayout()
        {
            // 1. Show a simple window
            // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
            {
                ImGui.Text("Hello, world!");
                ImGui.SliderFloat("Cam.Speed", ref speed, -5f, 5f, string.Empty);
                ImGui.SliderFloat("Cam.Pos.X", ref camera.Position.X, -10f, 10f, string.Empty);
                ImGui.SliderFloat("Cam.Pos.Y", ref camera.Position.Y, -10f, 10f, string.Empty);
                ImGui.SliderFloat("Cam.Pos.Z", ref camera.Position.Z, -10f, 10f, string.Empty);
                ImGui.SliderFloat("Cam.Pitch", ref camera.Rotation.X, 0f, 360f, string.Empty);
                ImGui.SliderFloat("Cam.Yaw", ref camera.Rotation.Y, 0f, 360f, string.Empty);
                ImGui.SliderFloat("Cam.Roll", ref camera.Rotation.Z, 0f, 360f, string.Empty);
                ImGui.SliderFloat("Cam.Zoom", ref camera.Zoom, 0.01f, 2.0f, string.Empty);
                ImGui.SliderFloat("Sprite.Sca;e", ref spriteScale, 1f, 5.0f, string.Empty);
                ImGui.Text(String.Format("Cam.Rotation: {0},{1},{2}, Zoom: {3}", camera.Rotation.X, camera.Rotation.Y, camera.Rotation.Z, camera.Zoom));
                ImGui.Text(String.Format("Cam.Position: {0},{1},{2}", camera.Position.X, camera.Position.Y, camera.Position.Z));
                ImGui.ColorEdit3("clear color", ref clear_color);
                if (ImGui.Button("Test Window")) show_test_window = !show_test_window;
                if (ImGui.Button("Another Window")) show_another_window = !show_another_window;
                ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
                ImGui.Text(string.Format("Mouse: {0},{1}", Mouse.GetState().X, Mouse.GetState().Y));
                //ImGui.InputText("Text input", _textBuffer, 100);

                //ImGui.Text("Texture sample");
                //ImGui.Image(_imGuiTexture, new Num.Vector2(300, 150), Num.Vector2.Zero, Num.Vector2.One, Num.Vector4.One, Num.Vector4.One); // Here, the previously loaded texture is used
            }

            // 2. Show another simple window, this time using an explicit Begin/End pair
            if (show_another_window)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.Begin("Another Window", ref show_another_window);
                ImGui.Text("Hello");
                ImGui.End();
            }

            // 3. Show the ImGui test window. Most of the sample code is in ImGui.ShowTestWindow()
            if (show_test_window)
            {
                ImGui.SetNextWindowPos(new Num.Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGui.ShowDemoWindow(ref show_test_window);
            }
        }

		public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
		{
			//initialize a texture
			var texture = new Texture2D(device, width, height);

			//the array holds the color for each pixel in the texture
			Color[] data = new Color[width * height];
			for(var pixel = 0; pixel < data.Length; pixel++)
			{
				//the function applies the color according to the specified pixel
				data[pixel] = paint( pixel );
			}

			//set the color
			texture.SetData( data );

			return texture;
		}
    }
}
