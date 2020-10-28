using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGame {

public class UI {

    public abstract class Element {

        public static float Opacity = 1f;
        public Rectangle bounds;
        public Element() {
            bounds = new Rectangle();
        }

    }

    public class MenuBar : Element {

        private List<MenuBarItem> children;

        public MenuBar(int x, int y, int w, int h = 0) : base() {
            this.bounds.X = x; this.bounds.Y = y;
            this.bounds.Width = w; this.bounds.Height = h; 
            children = new List<MenuBarItem>();
        }

        public MenuBar AddMenuBarItem(MenuBarItem item) {
            children.Add(item);
            return this;
        }

        public void Update(MouseState mouseState, MouseState previousMouseState) {
            foreach(MenuBarItem item in children) {
                item.Update(mouseState, previousMouseState);
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont uiFont, Texture2D pixel) {
            spriteBatch.Draw(texture: pixel, destinationRectangle: this.bounds, sourceRectangle: null, color: Color.Black * Opacity, rotation: 0, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: 0f);
            foreach(MenuBarItem item in children) {
                item.Draw(spriteBatch, uiFont, pixel);
            }
        }
        
        public void CalculateChildrenBounds(SpriteFont uiFont) {
            int horizontalOffset = this.bounds.X;
            int verticalOffset = this.bounds.Y;
            int maxHeight = 0;
            foreach(MenuBarItem item in children) {
                Vector2 textSize = uiFont.MeasureString(item.text);
                item.drawPosition = new Vector2(
                    horizontalOffset + MenuBarItem.HPadding,
                    verticalOffset + MenuBarItem.VPadding
                );
                item.bounds.X = horizontalOffset;
                item.bounds.Y = verticalOffset;
                item.bounds.Width = 2*MenuBarItem.HPadding + (int)Math.Ceiling(textSize.X);
                item.bounds.Height = 2*MenuBarItem.VPadding + (int)Math.Ceiling(textSize.Y);

                item.CalculateChildrenBounds(item.bounds, uiFont);

                horizontalOffset += item.bounds.Width;
                maxHeight = Math.Max(maxHeight, item.bounds.Height);
            }

            this.bounds.Height = maxHeight;
        }
    }

    public class MenuBarItem : Element {
        public static int HPadding = 7;
        public static int VPadding = 2;
        public string text;
        public Vector2 drawPosition;
        protected OnClick onClick;
        public bool mouseHovering;

        public List<MenuItem> childMenuItems;

        protected bool childrenOpen;

        public MenuBarItem(string text, OnClick onClick) : base() {
            this.text = text;
            this.onClick = onClick;

            childMenuItems = new List<MenuItem>();
        }

        public MenuBarItem AddMenuItem(MenuItem item) {
            childMenuItems.Add(item);
            return this;
        }

        public virtual void CalculateChildrenBounds(Rectangle parentBounds, SpriteFont uiFont) {
            int horizontalOffset = parentBounds.X;
            int verticalOffset = parentBounds.Y + parentBounds.Height;

            int textHeight = (int)Math.Ceiling(uiFont.MeasureString(text).Y); // parent text size == child text size
            int maxTextWidth = 0;
            foreach(MenuItem item in childMenuItems) {
                maxTextWidth = Math.Max(maxTextWidth, (int)Math.Ceiling(uiFont.MeasureString(item.text).X));
            }

            foreach(MenuItem item in childMenuItems) {
                Vector2 textSize = uiFont.MeasureString(item.text);
                item.drawPosition = new Vector2(
                    horizontalOffset + MenuItem.HPadding,
                    verticalOffset + MenuBarItem.VPadding
                );
                item.bounds.X = horizontalOffset;
                item.bounds.Y = verticalOffset;
                item.bounds.Width = maxTextWidth + MenuItem.RPadding;
                item.bounds.Height = textHeight + 2*MenuItem.VPadding;

                item.CalculateChildrenBounds(item.bounds, uiFont);
                verticalOffset += item.bounds.Height;
            }
        }

        public void Update(MouseState mouseState, MouseState previousMouseState) {
            if(mouseState.X >= this.bounds.X && mouseState.X <= (this.bounds.Width + this.bounds.X) &&
                mouseState.Y >= this.bounds.Y && mouseState.Y <= (this.bounds.Y + this.bounds.Height)) {
                mouseHovering = true;
            } else {
                mouseHovering = false;
            }

            bool mouseClicked = previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released;
            if(mouseHovering && mouseClicked) {
                if(childMenuItems.Count > 0) childrenOpen = true;
                else onClick();
            }

            bool childrenActive = false;
            if(childrenOpen) {
                foreach(MenuItem item in childMenuItems) {
                   if(item.Update(mouseState, previousMouseState)) {
                       childrenActive = true;
                   }
                }
            }

            if(mouseClicked && !mouseHovering && !childrenActive) {
                childrenOpen = false;
            }

        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont uiFont, Texture2D pixel) {
            if(mouseHovering || childrenOpen) {
                spriteBatch.Draw(texture: pixel, destinationRectangle: this.bounds, sourceRectangle: null, color: Color.Blue * Opacity, rotation: 0, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: 0f);
                spriteBatch.DrawString(spriteFont: uiFont, text: text, position: drawPosition, color: Color.White);

                if(childrenOpen) {
                    foreach(MenuItem item in childMenuItems) {
                        item.Draw(spriteBatch, uiFont, pixel);
                    }
                }
            } else {
                spriteBatch.DrawString(spriteFont: uiFont, text: text, position: drawPosition, color: Color.White); 
            }
        }

        public delegate void OnClick();
    }

    public class MenuItem : MenuBarItem {
        public static new int VPadding = 1;
        public static new int HPadding = 3;
        public static int RPadding = 15;
        public MenuItem(string text, OnClick onClick): base(text, onClick) {}

        public new MenuItem AddMenuItem(MenuItem item) {
            childMenuItems.Add(item);
            return this;
        }

        public new bool Update(MouseState mouseState, MouseState previousMouseState) {
            bool keepParentActive = false;
            if(mouseState.X >= this.bounds.X && mouseState.X <= (this.bounds.Width + this.bounds.X) &&
                mouseState.Y >= this.bounds.Y && mouseState.Y <= (this.bounds.Y + this.bounds.Height)) {
                mouseHovering = true;
            } else {
                mouseHovering = false;
            }

            bool mouseClicked = previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released;

            if(mouseHovering && childMenuItems.Count > 0) {
                childrenOpen = true;   
            }

            bool childrenActive = false;
            if(childrenOpen) {
                foreach(MenuItem item in childMenuItems) {
                    if(item.Update(mouseState, previousMouseState)) {
                        childrenActive = true;
                    }
                }
            }
            
            if(mouseHovering || childrenActive) {
                keepParentActive = true;
            }

            if(mouseClicked && mouseHovering && childMenuItems.Count == 0) {
                keepParentActive = false;
                childrenOpen = false;
                onClick();
            }

            if(!mouseHovering && !childrenActive) {
                childrenOpen = false;
            }
            
            return keepParentActive;
        }

        public new void Draw(SpriteBatch spriteBatch, SpriteFont uiFont, Texture2D pixel) {
            Color backgroundColor = (mouseHovering || childrenOpen) ? Color.Blue : Color.Black;
            backgroundColor *= Opacity;
            spriteBatch.Draw(texture: pixel, destinationRectangle: this.bounds, sourceRectangle: null, color: backgroundColor, rotation: 0, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: 0f);
            spriteBatch.DrawString(spriteFont: uiFont, text: text, position: drawPosition, color: Color.White);

            if(childrenOpen) {
                foreach(MenuItem item in childMenuItems) {
                    item.Draw(spriteBatch, uiFont, pixel);
                }
            }
        }

        public override void CalculateChildrenBounds(Rectangle parentBounds, SpriteFont uiFont) {
            int horizontalOffset = parentBounds.X + parentBounds.Width;
            int verticalOffset = parentBounds.Y;

            int textHeight = (int)Math.Ceiling(uiFont.MeasureString(text).Y); // parent text size == child text size
            int maxTextWidth = 0;
            foreach(MenuItem item in childMenuItems) {
                maxTextWidth = Math.Max(maxTextWidth, (int)Math.Ceiling(uiFont.MeasureString(item.text).X));
            }

            foreach(MenuItem item in childMenuItems) {
                Vector2 textSize = uiFont.MeasureString(item.text);
                item.drawPosition = new Vector2(
                    horizontalOffset + MenuItem.HPadding,
                    verticalOffset + MenuBarItem.VPadding
                );
                item.bounds.X = horizontalOffset;
                item.bounds.Y = verticalOffset;
                item.bounds.Width = maxTextWidth + MenuItem.RPadding;
                item.bounds.Height = textHeight + 2*MenuItem.VPadding;

                item.CalculateChildrenBounds(item.bounds, uiFont);
                verticalOffset += item.bounds.Height;
            }
        }
    }
}

}