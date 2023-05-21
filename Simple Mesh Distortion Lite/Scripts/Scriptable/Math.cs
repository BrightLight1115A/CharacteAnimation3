using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshDistortLite
{
    /// <summary>
    /// Math functions
    /// </summary>
    public class Math : ScriptableObject
    {

        /// <summary>
        /// Repeat a value between min and max
        /// </summary>
        /// <param name="num">Number to repeat</param>
        /// <param name="min">Minimum value it can get</param>
        /// <param name="max">Maximum value it can get</param>
        /// <returns>The value between min and max</returns>
        public static float Repeat(float num, float min, float max)
        {
            if (num < min)
                return max - (min - num) % (max - min);
            else
                return min + (num - min) % (max - min);

        }
        /// <summary>
        /// Repeat a number from and back between min and max
        /// </summary>
        /// <param name="num">Number to ping pong</param>
        /// <param name="min">Minimun number</param>
        /// <param name="max">Maximum Number</param>
        /// <returns>The value between min and max</returns>
        public static float PingPong(float num, float min, float max)
        {
            min = Repeat(num, min, 2 * max);
            if (min < max)
                return min;
            else
                return 2 * max - min;

        }
    }

}