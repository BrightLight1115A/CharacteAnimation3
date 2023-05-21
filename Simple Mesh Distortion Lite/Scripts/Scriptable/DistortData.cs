using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshDistortLite
{
    /// <summary>
    /// Hold all the information of a distortion and apply its calculations on the distortion.
    /// </summary>
    [System.Serializable]
    public class DistortData
    {
        
        

        /// <summary>
        /// If this distortion will be calculated
        /// </summary>
        public bool enabled = true;


        /// <summary>
        /// Name of the effect
        /// </summary>
        public string name = "Effect";

        /// <summary>
        /// Multiplier for the DistortAnimation, will make animation faster or slower
        /// </summary>
        public float animationSpeed = 1f;
        /// <summary>
        /// Type of this distortion
        /// </summary>
        public Distort.Type type;
        /// <summary>
        /// How much force is applied to the distortion
        /// </summary>
        public float force = 1f;
        
        /// <summary>
        /// Displacement for each vertice (only for calculation), used for animation
        /// </summary>
        public float movementDisplacement = 0;

        
        /// <summary>
        /// How much times the distortion will be applied from Bound.min to Bound.max
        /// </summary>
        public Vector3 tile = Vector3.one;

        /// <summary>
        /// Force for a distortion in the axis X, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceX = new AnimationCurve();
        /// <summary>
        /// Force for a distortion in the axis Y, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceY = new AnimationCurve();
        /// <summary>
        /// Force for a distortion in the axis Z, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceZ = new AnimationCurve();

        /// <summary>
        /// Change the value of the X axis of the vertice by its Y value, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceXY = new AnimationCurve();
        /// <summary>
        /// Change the value of the X axis of the vertice by its Z value, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceXZ = new AnimationCurve();
        /// <summary>
        /// Change the value of the Y axis of the vertice by its X value, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceYX = new AnimationCurve();
        /// <summary>
        /// Change the value of the Y axis of the vertice by its Z value, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceYZ = new AnimationCurve();
        /// <summary>
        /// Change the value of the Z axis of the vertice by its X value, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceZX = new AnimationCurve();
        /// <summary>
        /// Change the value of the Z axis of the vertice by its Y value, affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve displacedForceZY = new AnimationCurve();

        /// <summary>
        /// Force for a distortion in the axis X, NOT affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve staticForceX = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        /// <summary>
        /// Force for a distortion in the axis Y, NOT affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve staticForceY = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        /// <summary>
        /// Force for a distortion in the axis Z, NOT affected by the movementDisplacement param 
        /// </summary>
        public AnimationCurve staticForceZ = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        /// <summary>
        /// Calculate vertice position inside the bounds using the pingpong algorithm
        /// </summary>
        public bool isPingPong = true;


        /// <summary>
        /// Hide or show the foldout in the editor screen for this distortion
        /// </summary>
        public bool showInEditor = true;

        /// <summary>
        /// Calculate this distortion in world or local space.
        /// </summary>
        public bool calculateInWorldSpace = false;


        /// <summary>
        /// Used in the Distort method to hold the distortion value of X
        /// </summary>
        float x = 0;
        /// <summary>
        /// Used in the Distort method to hold the distortion value of Y
        /// </summary>
        float y = 0;
        /// <summary>
        /// Used in the Distort method to hold the distortion value of Z
        /// </summary>
        float z = 0;

        /// <summary>
        /// Hold the bound for the mesh, to calculate the distortion for the mesh.
        /// </summary>
        //Bounds bounds;
        /// <summary>
        /// Hold the minimum value of the bound for calculation
        /// </summary>
        Vector3 bMin;
        /// <summary>
        /// Hold the maximum value of the bound for calculation
        /// </summary>
        //Vector3 bMax;
        /// <summary>
        /// Hold the (max - min) value of the bound for calculation
        /// </summary>
        Vector3 bNormalized;
        /// <summary>
        /// Hold the center position of the bounds
        /// </summary>
        //Vector3 bCenter;
        
        

        /// <summary>
        /// Set the bounds of the mesh to use in the calculations later.
        /// </summary>
        /// <param name="bounds"></param>
        public void SetBounds(Bounds bounds)
        {
            bMin = bounds.min;
            //bMax = bounds.max;
            //bCenter = bounds.center;
            bNormalized = bounds.max - bounds.min;

            if (bNormalized.x == 0)
                bNormalized.x = 0.1f;
            if (bNormalized.y == 0)
                bNormalized.y = 0.1f;
            if (bNormalized.z == 0)
                bNormalized.z = 0.1f;

            //this.bounds = bounds;
        }

        /// <summary>
        /// Used in the DistortVertice method, hold the position of a vertice inside the bounds (0-1)
        /// </summary>
        Vector3 percentage;
        /// <summary>
        /// Using the static force for each axis this hold the multiplier force for a vertice depending of its axis.
        /// </summary>
        float multiplier;

        /// <summary>
        /// Used to hold the direction of the vertice, used in some calculations like Inflate or spin.
        /// </summary>
        Vector3 dir;

        /// <summary>
        /// Calculate the distortion in a position
        /// </summary>
        /// <param name="vertice">Position to calculate the distortion</param>
        public void DistortVertice(ref Vector3 vertice)
        {
            //Distortion values start at zero
            x = 0;
            y = 0;
            z = 0;

            //Set the point param to start the calculations for it.
            percentage = vertice;

            if (calculateInWorldSpace)
            {
                //How much force this distortion will have depending of its position
                multiplier =
                    staticForceX.Evaluate((percentage.x - bMin.x) / bNormalized.x) *
                    staticForceY.Evaluate((percentage.y - bMin.y) / bNormalized.y) *
                    staticForceZ.Evaluate((percentage.z - bMin.z) / bNormalized.z);

                //Point will become between 0 and 1
                percentage.x /= bNormalized.x;
                percentage.y /= bNormalized.y;
                percentage.z /= bNormalized.z;
            }
            //Set point to local space
            else
            {
                percentage.x -= bMin.x;
                percentage.y -= bMin.y;
                percentage.z -= bMin.z;

                //Point will become between 0 and 1
                percentage.x /= bNormalized.x;
                percentage.y /= bNormalized.y;
                percentage.z /= bNormalized.z;

                //How much force this distortion will have depending of its position
                multiplier = staticForceX.Evaluate(percentage.x) * staticForceY.Evaluate(percentage.y) * staticForceZ.Evaluate(percentage.z);
            }

            

            //Force the point to be inside the bounds using the ping pong algorithm
            if (isPingPong)
            {
                //Set the point using its axis, displacement and tile, and ping pong it between 0 and 1
                percentage.x = Math.PingPong((percentage.x + movementDisplacement) * tile.x, 0, 1);
                percentage.y = Math.PingPong((percentage.y + movementDisplacement) * tile.y, 0, 1);
                percentage.z = Math.PingPong((percentage.z + movementDisplacement) * tile.z, 0, 1);

            }
            else
            {
                //Set the point using its axis, displacement and tile
                percentage.x = (percentage.x + movementDisplacement) * tile.x;
                percentage.y = (percentage.y + movementDisplacement) * tile.y;
                percentage.z = (percentage.z + movementDisplacement) * tile.z;
            }
            

           
            //Set the force in a axis using another axis
            x += displacedForceXY.Evaluate(percentage.y) * force;
            x += displacedForceXZ.Evaluate(percentage.z) * force;

            y += displacedForceYX.Evaluate(percentage.x) * force;
            y += displacedForceYZ.Evaluate(percentage.z) * force;

            z += displacedForceZX.Evaluate(percentage.x) * force;
            z += displacedForceZY.Evaluate(percentage.y) * force;
             
            


            //Aplly the distortion to the vertice value
            vertice.x += x * multiplier;
            vertice.y += y * multiplier;
            vertice.z += z * multiplier;

         
        }

    }

}