using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public class BrownianBridge
    {
        #region private fields
        private readonly double[] dates;
        private readonly int[] simulationIndexes;
        private readonly double[] simulationStdDevs;
        private readonly int[] leftIndexes;
        private readonly int[] rightIndexes;
        private readonly double[] leftWeigths;
        private readonly double[] rightWeigths;
        #endregion
        #region private methods
        private BrownianBridge(double[] dates, int[] simulationIndexes, double[] simulationStdDevs,
                               int[] leftIndexes, int[] rightIndexes)
        {
            this.dates = dates;
            this.simulationIndexes = simulationIndexes;
            this.simulationStdDevs = simulationStdDevs;
            this.leftIndexes = leftIndexes;
            this.rightIndexes = rightIndexes;

            leftWeigths = new double[leftIndexes.Length];
            rightWeigths = new double[rightIndexes.Length];
            for (int i = 0; i < simulationIndexes.Length; ++i)
            {
                int currentIndex = simulationIndexes[i];
                int leftIndex = leftIndexes[currentIndex];
                int rightIndex = rightIndexes[currentIndex];

                if (rightIndex != dates.Length)
                {
                    if (leftIndex == -1)
                    {
                        leftWeigths[i] = (dates[rightIndex] - dates[currentIndex]) / (dates[rightIndex] - 0.0);
                        rightWeigths[i] = (dates[currentIndex] - 0.0) / (dates[rightIndex] - 0.0);
                    }
                    else
                    {
                        leftWeigths[i] = (dates[rightIndex] - dates[currentIndex]) / (dates[rightIndex] - dates[leftIndex]);
                        rightWeigths[i] = (dates[currentIndex] - dates[leftIndex]) / (dates[rightIndex] - dates[leftIndex]);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Brownian bridge constructor
        /// </summary>
        /// <param name="dates"> Simulation dates, assumed to be sorted and strictly positive </param>
        public static BrownianBridge Create(double[] dates)
        {
            return Create(dates, Enumerable.Range(0, dates.Length).ToArray());
        }

        /// <summary>
        /// Brownian bridge constructor
        /// </summary>
        /// <param name="dates"> Simulation dates, assumed to be sorted and strictly positive </param>
        /// <param name="importantIndexes"> Indexes of Dates that should choose first in brownian bridge construction </param>
        public static BrownianBridge Create(double[] dates, int[] importantIndexes)
        {
            if (importantIndexes.Any(index => index > dates.Length))
                throw new Exception("BrownianBridge : importantIndexes contain an index higher that dates.Length !");
            
            if (!EnumerableUtils.IsSorted(dates))
                throw new Exception("BrownianBridge : dates must be sorted ! ");
            
            if (dates[0] < 0.0)
                throw new Exception("BrownianBridge : First date must be positive");

            int[] simulationIndexes = EnumerableUtils.ConstantArray(int.MinValue, dates.Length);
            int[] leftIndex = EnumerableUtils.ConstantArray(-1, dates.Length);
            int[] rightIndex = EnumerableUtils.ConstantArray(dates.Length, dates.Length);
            double[] simulVariance = EnumerableUtils.ConstantArray(double.NaN, dates.Length);

            double[] dateVariances = EnumerableUtils.Map(dates, d => d);
            var importantIndexesList = new List<int>(importantIndexes);

            for (int k = 0; k < dates.Length; ++k)
            {
                int currentIndex;
                if (importantIndexes.Count() > 0)
                {
                    double maxVariance = importantIndexesList.Select(i => dateVariances[i]).Max();
                    currentIndex = importantIndexesList.First(i => DoubleUtils.MachineEquality(dateVariances[i], maxVariance));
                    importantIndexesList.Remove(currentIndex);
                }
                else
                {
                    double maxVariance =  dateVariances.Max();
                    currentIndex = dateVariances.Select((v, i) => new {Var = v, Index = i})
                        .Where(v => DoubleUtils.MachineEquality(v.Var, maxVariance))
                        .Select(v => v.Index)
                        .First();
                }

                var currentVariance = dateVariances[currentIndex];
                simulationIndexes[k] = currentIndex;
                simulVariance[k] = currentVariance;

                int currentLeftIndex = leftIndex[currentIndex];
                int currentRightIndex = rightIndex[currentIndex];
                double leftDate = currentLeftIndex == -1 ? 0.0 : dates[currentLeftIndex];
                double currentDate = dates[currentIndex];
                double rightDate = dates[Math.Min(currentRightIndex, dates.Length - 1)];

                for (int j = currentLeftIndex + 1; j < currentIndex; ++j)
                {
                    dateVariances[j] = (dates[j] - leftDate) * (currentDate - dates[j]) / (currentDate - leftDate);
                    rightIndex[j] = currentIndex;
                }
                dateVariances[currentIndex] = 0.0;
                for (int j = currentIndex + 1; j < currentRightIndex; ++j)
                {
                    dateVariances[j] = (rightDate - dates[j]) * (dates[j] - currentDate) / (rightDate - currentDate);
                    leftIndex[j] = currentIndex;
                }
            }
            var simulationStdDevs = simulVariance.Select(Math.Sqrt).ToArray();
            return new BrownianBridge(dates, simulationIndexes, simulationStdDevs, leftIndex, rightIndex);
        }

        public double[] NextPath(double[] gaussians)
        {
            Debug.Assert(gaussians.Length == dates.Length);
            var path = new double[dates.Length];
            for (int i = 0; i < simulationIndexes.Length; ++i)
            {
                int currentIndex = simulationIndexes[i];
                int leftIndex = leftIndexes[currentIndex];
                int rightIndex = rightIndexes[currentIndex];

                var leftVal = (leftIndex == -1) ? 0.0 : path[leftIndex];
                var rightVal = (rightIndex == dates.Length) ? 0.0 : path[rightIndex];
                path[currentIndex] = leftVal * leftWeigths[i] + rightVal * rightWeigths[i] + gaussians[i] * simulationStdDevs[i];
            }
            return path;
        }
        public double[][] NextPath(double[] gaussians, int dimension)
        {
            Debug.Assert(gaussians.Length == dates.Length * dimension);
            var path = new double[dates.Length][];
            
            int gaussIndex = 0;
            for (int i = 0; i < simulationIndexes.Length; ++i)
            {
                int currentIndex = simulationIndexes[i];
                
                int leftIndex = leftIndexes[currentIndex];
                var leftVal = (leftIndex == -1) ? new double[dimension] : path[leftIndex];
                
                int rightIndex = rightIndexes[currentIndex];
                var rightVal = (rightIndex == dates.Length) ? new double[dimension] : path[rightIndex];

                var currentVal = new double[dimension];
                for (int k = 0; k < dimension; k++)
                {
                    currentVal[k] = leftVal[k] * leftWeigths[i] + rightVal[k] * rightWeigths[i] + gaussians[gaussIndex++] * simulationStdDevs[i];
                }
                path[currentIndex] = currentVal;
            }
            return path;
        }
        public int GaussianSize(int dimension)
        {
            return dates.Length * dimension;
        }
        public double[] Dates
        {
            get { return dates; }
        }
    }
}
