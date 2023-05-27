using System;

namespace Utillities {
    /// <summary>
    /// Provides a wide range of methods and functions
    /// </summary>
    public static class ExtendedMath {
        /// <summary>
        /// Calculates the linear step value based on the input parameters.
        /// </summary>
        /// <param name="x">The input value.</param>
        /// <param name="n">The power value (default: 1).</param>
        /// <param name="edge0">The lower edge value (default: 0).</param>
        /// <param name="edge1">The upper edge value (default: 1.0f).</param>
        /// <returns>The linear step value.</returns>
        public static float LinearStep(float x, int n = 1, float edge0 = 0, float edge1 = 1.0f) => Clamp((x - edge0) / (edge1 - edge0));

        /// <summary>
        /// Calculates the root step value based on the input parameters.
        /// </summary>
        /// <param name="x">The input value.</param>
        /// <param name="n">The power value (default: 1).</param>
        /// <param name="edge0">The lower edge value (default: 0).</param>
        /// <param name="edge1">The upper edge value (default: 1.0f).</param>
        /// <returns>The root step value.</returns>
        public static float RootStep(float x, int n = 1, float edge0 = 0, float edge1 = 1.0f) => Clamp(MathF.Sqrt((x - edge0) / (edge1 - edge0)));

        /// <summary>
        /// Calculates the smooth step value based on the input parameters.
        /// </summary>
        /// <param name="x">The input value.</param>
        /// <param name="n">The power value (default: 1).</param>
        /// <param name="edge0">The lower edge value (default: 0).</param>
        /// <param name="edge1">The upper edge value (default: 1.0f).</param>
        /// <returns>The smooth step value.</returns>
        public static float SmoothStep(float x, int n = 1, float edge0 = 0, float edge1 = 1.0f) {
            x = Clamp((x - edge0) / (edge1 - edge0));
            float result = 0;
            for (int i = 0; i <= n; ++i) {
                result += PascalTriangle(-n - 1, i) * PascalTriangle(2 * n + 1, n - i) * MathF.Pow(x, n + i + 1);
            }
            return result;
        }

        /// <summary>
        /// Clamps the specified value between the given lower and upper limits.
        /// </summary>
        /// <param name="x">The value to clamp.</param>
        /// <param name="lowerlimit">The lower limit (default: 0.0f).</param>
        /// <param name="upperlimit">The upper limit (default: 1.0f).</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(float x, float lowerlimit = 0.0f, float upperlimit = 1.0f) => (x < lowerlimit) ? (lowerlimit) : ((x > upperlimit) ? (upperlimit) : (x));

        /// <summary>
        /// Calculates the value of the Pascal triangle coefficient for the given parameters.
        /// </summary>
        /// <param name="a">The first parameter.</param>
        /// <param name="b">The second parameter.</param>
        /// <returns>The Pascal triangle coefficient value.</returns>
        public static float PascalTriangle(int a, int b) {
            float result = 1;
            for (int i = 0; i < b; i++) {
                result *= (a - i) / (a + i);
            }
            return result;
        }
    }
}
