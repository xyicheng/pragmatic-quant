using System;
using ExcelDna.Integration;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Sobol;

namespace pragmatic_quant_com
{
    public class MathFunctions
    {
        [ExcelFunction(Description = "Generate uniform sample with Sobol algorithm", Category = "PragmaticQuant_Math")]
        public static object Sobol(string directionType, int dimension, int nbPaths,
            int nbSkippedPaths, object startDimIndexDisplay)
        {
            try
            {
                SobolDirection direction;
                if (!Enum.TryParse(directionType, true, out direction))
                    throw new Exception(string.Format("Unknow sobol direction type {0}", directionType));

                var sobolGen = new SobolRsg(dimension, direction);
                sobolGen.SkipTo((ulong) nbSkippedPaths);

                int startIndex;
                if (!NumberConverter.TryConvertInteger(startDimIndexDisplay, out startIndex))
                {
                    startIndex = 0;
                }
                if (startIndex >= dimension)
                    throw new Exception(string.Format("invalid startDimIndexDisplay : {0}", startIndex));

                var result = new double[nbPaths, dimension - startIndex];
                for (int i = 0; i < nbPaths; i++)
                {
                    var sample = sobolGen.NextSequence();
                    for (int j = 0; j < dimension - startIndex; j++)
                        result[i, j] = sample[startIndex + j];
                }
                return result;
            }
            catch (Exception e)
            {
                return string.Format("FAILURE '{0}'", e.Message);
            }
        }

        [ExcelFunction(Description = "Cubic spline interpolation", Category = "PragmaticQuant_Math")]
        public static object CubicSpline(double[] abscissae, double[] values, double[] points)
        {
            try
            {
                var interpoler = new CubicSplineInterpoler(abscissae, values);

                var result = new double[points.Length, 1];
                for (int i = 0; i < points.Length; i++)
                {
                    result[i, 0] = interpoler.Eval(points[i]);
                }
                return result;
            }
            catch (Exception e)
            {
                return string.Format("FAILURE '{0}'", e.Message);
            }
        }
    }
}