using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contain the scripts for the Easy Distortion Package
/// </summary>

namespace MeshDistortLite
{


    /// <summary>
    /// Animate distortions and save animations for it.
    /// </summary>
    [RequireComponent(typeof(Distort))]
    public class AnimatedDistort : MonoBehaviour {

        /// <summary>
        /// Used to configure a new animation to be saved
        /// </summary>
        public float animationFramesPerSec = 30;

        /// <summary>
        /// Number of frames to save in a animation
        /// </summary>
        public int animationFrames = 1;

        /// <summary>
        /// Reference to the DistortVertices on this GameObject
        /// </summary>
        protected Distort distort;

        /// <summary>
        /// Types of animations it can do
        /// </summary>
        public enum Animate
        {
            force,
            displacement
        }

        /// <summary>
        /// Animation options
        /// </summary>
        public enum Type
        {
            constant,
            pingpong,
            sin,
            curve
        }

        /// <summary>
        /// Current animation type
        /// </summary>
        public Type type = Type.constant;
        public Animate animate = Animate.displacement;
        public AnimationCurve curveType;

        /// <summary>
        /// Speed of the animation of constant type
        /// </summary>
        public float constantSpeed = 1;

        /// <summary>
        /// Min value for not const animation
        /// </summary>
        public float minValue = 0;
        /// <summary>
        /// Max value for not const animation
        /// </summary>
        public float maxValue = 10;

        
        /// <summary>
        /// Index of the animation that is playing. 0 is a special value to calculate animation in real time
        /// </summary>
        public int playAnimationIndex = 0;

        /// <summary>
        /// Animation index, but remove the special index of 0 and fix the index.
        /// </summary>
        public int currentAnimation
        {
            get
            {
                return playAnimationIndex - 1;
            }
            set
            {
                playAnimationIndex = value + 1;
            }
        }

    

        /// <summary>
        /// Is the script changing one animation to another?
        /// </summary>
        public bool updatingAnimation = false;



        public bool isPlaying = true;

        void Start() {
            Setup();
        }

        public void Play()
        {
            isPlaying = true;
            playingAnimationTime = 0;
        }
        public void Stop()
        {
            isPlaying = false;
            playingAnimationTime = 0;
        }

        float playingAnimationTime = 0;
        void LateUpdate()
        {
            if (isPlaying)
            {
                //Check if it is currently doing some animation of its own
                if (!updatingAnimation)
                {
                    //If is marked to calculate dinamic
                    if (playAnimationIndex == 0)
                    {
                        //Do animation in real time
                        Animation(playingAnimationTime, Time.deltaTime);
                        playingAnimationTime += Time.deltaTime;
                    }
                    
                }
            }
        }

        /// <summary>
        /// Do the script setup
        /// </summary>
        void Setup()
        {
            ///Get the DistortVertices of this gameobject
            distort = GetComponent<Distort>();
            ///Make the mesh dynamic, since we will be updating the vertices every frame
            distort.MakeDynamic();
        }

        /// <summary>
        /// Calculate the distortion in real time
        /// </summary>
        public void CalculateInRealTime()
        {
            playAnimationIndex = 0;
        }


        /// <summary>
        /// Calculate a animation in a determined frame
        /// </summary>
        /// <param name="displaceOffset">Time in animation</param>
        /// <param name="delta">Delta time since last animation update</param>
        void Animation(float displaceOffset, float delta) {
            //Call setup if needed
            if (distort == null)
                Setup();

            ///For each distortion in the GameObject update the movementDisplacement, this will make the object animate
            for (int i = 0; i < distort.distort.Count; i++)
            {
                //Animation type constant
                if (type == Type.constant)
                {
                    if (animate == Animate.displacement)
                    {
                        float disp = constantSpeed * delta * distort.distort[i].animationSpeed;
                        distort.distort[i].movementDisplacement += disp;
                    }
                    else
                    {
                        distort.distort[i].force += constantSpeed * delta * distort.distort[i].animationSpeed;
                    }
                }
                //Animation type pingpong
                else if (type == Type.pingpong)
                {
                    if (animate == Animate.displacement)
                    {
                        float disp = Mathf.Lerp(minValue, maxValue, Mathf.PingPong(displaceOffset * constantSpeed * distort.distort[i].animationSpeed, 1));
                        distort.distort[i].movementDisplacement = disp;
                    }
                    else
                    {
                        distort.distort[i].force = Mathf.Lerp(minValue, maxValue, Mathf.PingPong(displaceOffset * constantSpeed * distort.distort[i].animationSpeed, 1));
                    }
                }
                //Animation type sin
                else if (type == Type.sin)
                {
                    if (animate == Animate.displacement)
                    {
                        float disp = Mathf.Lerp(minValue, maxValue, (Mathf.Sin(displaceOffset * constantSpeed * distort.distort[i].animationSpeed) + 1) * 0.5f);
                        distort.distort[i].movementDisplacement = disp;
                    }
                    else
                    {
                        distort.distort[i].force = Mathf.Lerp(minValue, maxValue, (Mathf.Sin(displaceOffset * constantSpeed * distort.distort[i].animationSpeed) + 1) * 0.5f);
                    }
                }
                else if (type == Type.curve)
                {
                    if (animate == Animate.displacement)
                    {
                        float disp = curveType.Evaluate(displaceOffset * constantSpeed * distort.distort[i].animationSpeed);
                        distort.distort[i].movementDisplacement = disp;
                    }
                    else
                    {
                        distort.distort[i].force = curveType.Evaluate(displaceOffset * constantSpeed * distort.distort[i].animationSpeed);
                    }
                }

               
            }
            //Update the mesh vertices position.
            distort.UpdateDistort();
        }

       

       

       
       
      

        

      
    }



}


