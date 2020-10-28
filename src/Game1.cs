using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImGuiNET;
using System;
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
        private RenderTarget2D _finalGameTarget;

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

       }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_finalGameTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(
               texture: pixel, 
                destinationRectangle: new Rectangle(10, 10, 100, 1),
                sourceRectangle: null,
                color: Color.GreenYellow,
                rotation: MathHelper.ToRadians(lineAngle),
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            _spriteBatch.End(); 

            /** Render final render target to screen */
            GraphicsDevice.SetRenderTarget(null);

             _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
             _spriteBatch.Draw(_finalGameTarget, new Rectangle(0,0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
             _spriteBatch.End();
            
            _imGuiRenderer.BeforeLayout(gameTime);
            ImGuiLayout();
            _imGuiRenderer.AfterLayout();
            
            base.Draw(gameTime);
        }

        private float f = 0.0f;

        private bool show_test_window = false;
        private bool show_another_window = false;
        private Num.Vector3 clear_color = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);
        private byte[] _textBuffer = new byte[100];

        private float lineAngle = 0f;

        protected virtual void ImGuiLayout()
        {
            // 1. Show a simple window
            // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
            {
                ImGui.Text("Hello, world!");
                ImGui.SliderFloat("Line Angle", ref lineAngle, 0f, 360.0f, string.Empty);
                ImGui.ColorEdit3("clear color", ref clear_color);
                if (ImGui.Button("Test Window")) show_test_window = !show_test_window;
                if (ImGui.Button("Another Window")) show_another_window = !show_another_window;
                ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

                ImGui.InputText("Text input", _textBuffer, 100);

                ImGui.Text("Texture sample");
                ImGui.Image(_imGuiTexture, new Num.Vector2(300, 150), Num.Vector2.Zero, Num.Vector2.One, Num.Vector4.One, Num.Vector4.One); // Here, the previously loaded texture is used
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
