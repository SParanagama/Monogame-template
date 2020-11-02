using Microsoft.Xna.Framework;
using System;
namespace Core
{

class Camera {

    public Vector3 Position;
    // Pitch, Yaw, Roll
    public Vector3 Rotation;
    public float Zoom;

    private float canvasWidth, canvasHeight, zNear, zFar;

    private Matrix scaleMatrix;
    private Matrix cameraRotationMatrix;
    private Matrix cameraTranslationMatrix;
    private Matrix screenSpaceTransformationMatrix;
    private Matrix projectionMatrix;
    private Matrix viewMatrix;

    public Camera(float canvasWidth, float canvasHeight, float zNear, float zFar) {
        Position = new Vector3();
        Rotation = new Vector3();
        scaleMatrix = new Matrix();
        cameraTranslationMatrix = new Matrix();
        cameraRotationMatrix = new Matrix();
        projectionMatrix = new Matrix();
        viewMatrix = new Matrix();
        /** View Transformed points are in Normallized Device Coordinates */
        /**
                   (0,1)
                     ^
                     |
                     |
         (-1,0) <----------> (1,0)
        **/
        /** These will be transformed to Screen Coordinates ( (0,0) in Upper Left ) */
        Matrix screenSpaceTransformColumnVector = new Matrix(
            canvasWidth/2f, 0f,              0f,  canvasWidth/2f,
            0f,            -canvasHeight/2f, 0f,  canvasHeight/2f,
            0f,             0f,             0.5f, 0.5f,
            0f,             0f,              0f,  1f
        );
        screenSpaceTransformationMatrix = new Matrix();
        Matrix.Transpose(ref screenSpaceTransformColumnVector, out screenSpaceTransformationMatrix);

        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        this.zNear = zNear;
        this.zFar = zFar;

    }

    public void DeriveWorldToScreenSpaceTransformationMatrix(float scale, out Matrix worldToScreenSpaceMatrix) {
        Matrix.CreateScale(scale, out scaleMatrix);
        Matrix.CreateTranslation(-Position.X, -Position.Y, -Position.Z, out cameraTranslationMatrix);
        Matrix.CreateFromYawPitchRoll(-MathHelper.ToRadians(Rotation.Y),  // Yaw 
                                      -MathHelper.ToRadians(Rotation.X),  // Pitch
                                      -MathHelper.ToRadians(Rotation.Z), out cameraRotationMatrix);
        float aspectRatio = canvasWidth/canvasHeight;
        float zoomedWidth = canvasWidth*Zoom;
        Matrix.CreateOrthographic(zoomedWidth, zoomedWidth/aspectRatio, zNear, zFar, out projectionMatrix);
        Matrix.Multiply(ref cameraTranslationMatrix, ref cameraRotationMatrix, out viewMatrix);
        worldToScreenSpaceMatrix = screenSpaceTransformationMatrix * projectionMatrix * viewMatrix * scaleMatrix;
    }

    public ref Matrix GetViewRotationMatrix() {
        return ref cameraRotationMatrix;
    }

}
}