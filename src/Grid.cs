using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Core
{

class Grid {

    public int GridSize;
    private List<Vector3> xPoints;
    private List<Vector3> yPoints;
    private Vector3 corner1;
    private Vector3 corner2;
    private Vector3 corner3;
    private Vector3 corner4;


    public Grid(int size) {
        float unit = 1f;
        
        GridSize = size;
        xPoints = new List<Vector3>();
        yPoints = new List<Vector3>();
        corner1 = new Vector3(-unit*size, -unit*size, 0f);  // (-1, -1)
        corner2 = new Vector3( unit*size, -unit*size, 0f);  // ( 1, -1)
        corner3 = new Vector3(-unit*size,  unit*size, 0f);  // (-1,  1)
        corner4 = new Vector3( unit*size,  unit*size, 0f);  // ( 1,  1)

        for(int i=0; i<size; i++) {
            // Lines parallel to X axis
            xPoints.Add(new Vector3(
                -unit*size, i*unit, 0f
            ));
            xPoints.Add(new Vector3(
                 unit*size, i*unit, 0f
            ));
            if(i != 0) {
                xPoints.Add(new Vector3(
                -unit*size, -i*unit, 0f
                ));
                xPoints.Add(new Vector3(
                 unit*size, -i*unit, 0f
                ));
            }

            // Lines parallel to Y Axis   
            yPoints.Add(new Vector3(
                i*unit, -unit*size, 0f
            ));
            yPoints.Add(new Vector3(
                i*unit, unit*size, 0f  
            ));
            if(i != 0) {
                yPoints.Add(new Vector3(
                   -i*unit, -unit*size, 0f
                ));
                yPoints.Add(new Vector3(
                   -i*unit, unit*size, 0f  
                ));   
            }
        }
    }
    
    public void Draw(ref Matrix worldToScreen, SpriteBatch spriteBatch, Texture2D pixel) {

        int numLines = 2*GridSize+1 - 2 ; // Per Axis without outer lines
        for(int i=0, j=0; i<numLines; i++, j+=2) {
            // Draw Lines Parallel to X Axis
            Vector3 start = xPoints[j];
            Vector3 end   = xPoints[j+1];

            DrawLine(ref start, ref end, ref worldToScreen, spriteBatch, pixel);

            // Draw Lines Parallel to Y Axis
            start = yPoints[j];
            end   = yPoints[j+1];

            DrawLine(ref start, ref end, ref worldToScreen, spriteBatch, pixel);

                        
        }
        
        // Draw Outer lines
        DrawLine(ref corner1, ref corner2, ref worldToScreen, spriteBatch, pixel);
        DrawLine(ref corner3, ref corner4, ref worldToScreen, spriteBatch, pixel);
        DrawLine(ref corner1, ref corner3, ref worldToScreen, spriteBatch, pixel);
        DrawLine(ref corner2, ref corner4, ref worldToScreen, spriteBatch, pixel);

    }

    private void DrawLine(ref Vector3 start, ref Vector3 end, 
                          ref Matrix worldToScreen, SpriteBatch spriteBatch, Texture2D pixel) {

        Vector3 transformedStart = new Vector3();
        Vector3 transformedEnd = new Vector3();
        Vector3.Transform(ref start, ref worldToScreen, out transformedStart);
        Vector3.Transform(ref end,   ref worldToScreen, out transformedEnd);

        //Console.WriteLine("Transformed Line: " + transformedStart.ToString() + ", " + transformedEnd.ToString());
        Vector3 lineVector = transformedStart - transformedEnd;
        Vector2 lineVector2D = new Vector2(lineVector.X, lineVector.Y);
        float lineAngle = (float)( Math.Atan2(transformedEnd.Y - transformedStart.Y, transformedEnd.X - transformedStart.X));
        spriteBatch.Draw(
            texture: pixel, 
            destinationRectangle: new Rectangle((int)transformedStart.X, (int)transformedStart.Y, (int)lineVector2D.Length(), 1),
            sourceRectangle: null,
            color: Color.GreenYellow,
            rotation: lineAngle,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );

        //spriteBatch.Draw(pixel, new Vector2(transformedStart.X, transformedStart.Y), null, Color.White, 0, Vector2.Zero, Vector2.One*5, SpriteEffects.None, 1f);
        //spriteBatch.Draw(pixel, new Vector2(transformedEnd.X, transformedEnd.Y), null, Color.White, 0, Vector2.Zero, Vector2.One*5, SpriteEffects.None, 1f);
    }
}

class Line {

    public Vector3 Start;
    public Vector3 End;
    private Vector3 transformedStart;
    private Vector3 transformedEnd;

    public Line(float x1, float y1, float z1,
                float x2, float y2, float z2) {

        Start = new Vector3(x1, y1, z1);
        End = new Vector3(x2, y2, z2);

        transformedStart = new Vector3();
        transformedEnd = new Vector3();
    }

    public void Update(ref Matrix worldToScreen, SpriteBatch spriteBatch, Texture2D pixel,
                       float canvasWidth, float canvasHeight) {
        


    }

    public void Draw(ref Vector3 start, ref Vector3 end,
                     ref Vector3 transformedStart, ref Vector3 transformedEnd,
                     ref Matrix worldToScreen, SpriteBatch spriteBatch, Texture2D pixel, float canvasWidth, float canvasHeight) {
        Vector3.Transform(position: ref start, matrix: ref worldToScreen,
                          result: out transformedStart);
        Vector3.Transform(position: ref end, matrix: ref worldToScreen,
                          result: out transformedEnd);
        
        /** Transformed 2D coordinates are in XNA 3D Normalized Device Coordinate (NDC) system */



        Console.WriteLine("Transformed Line: " + transformedStart.ToString() + ", " + transformedEnd.ToString());
        Vector3 lineVector = transformedEnd - transformedStart;
        float lineAngle = (float)( /**Math.PI -*/ Math.Atan2(transformedEnd.Y - transformedStart.Y, transformedEnd.X - transformedStart.X));
        spriteBatch.Draw(
            texture: pixel, 
            destinationRectangle: new Rectangle((int)transformedStart.X, (int)transformedEnd.X, (int)lineVector.Length(), 1),
            sourceRectangle: null,
            color: Color.GreenYellow,
            rotation: lineAngle,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }
}

}