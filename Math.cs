using System;

namespace Utillities {
    /// <summary>
    /// Provides a wide range of methods and functions
    /// </summary>
    public static class Math {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <param name="edge0"></param>
        /// <param name="edge1"></param>
        /// <returns></returns>
        public static float LinearStep(float x, int n = 1, float edge0 = 0, float edge1 = 1.0f) => Clamp((x - edge0) / (edge1 - edge0));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <param name="edge0"></param>
        /// <param name="edge1"></param>
        /// <returns></returns>
        public static float RootStep(float x, int n = 1, float edge0 = 0, float edge1 = 1.0f) => Clamp(MathF.Sqrt((x - edge0) / (edge1 - edge0)));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <param name="edge0"></param>
        /// <param name="edge1"></param>
        /// <returns></returns>
        public static float SmoothStep(float x, int n = 1, float edge0 = 0, float edge1 = 1.0f) {
            x = Clamp((x - edge0) / (edge1 - edge0));
            float result = 0;
            for (int i = 0; i <= n; ++i) {
                result += PascalTriangle(-n - 1, i) * PascalTriangle(2 * n + 1, n - i) * MathF.Pow(x, n + i + 1);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="lowerlimit"></param>
        /// <param name="upperlimit"></param>
        /// <returns></returns>
        public static float Clamp(float x, float lowerlimit = 0.0f, float upperlimit = 1.0f) => (x < lowerlimit) ? (lowerlimit) : ((x > upperlimit) ? (upperlimit) : (x));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float PascalTriangle(int a, int b) {
            float result = 1;
            for (int i = 0; i < b; i++) {
                result *= (a - i) / (a + i);
            }
            return result;
        }
    }
}
