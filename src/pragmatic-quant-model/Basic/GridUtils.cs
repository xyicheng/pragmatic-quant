﻿using System;

namespace pragmatic_quant_model.Basic
{
    public static class GridUtils
    {
        /// <summary>
        /// Build a regular grid with endpoints {start, end}.
        /// </summary>
        /// <param name="start">first point</param>
        /// <param name="end">last point</param>
        /// <param name="size">grid size (should be > 1) </param>
        /// <returns></returns>
        public static double[] RegularGrid(double start, double end, int size)
        {
            if (end <= start)
                throw new Exception("GridUtils : start must be lower than end");
            if (size < 2)
                throw new Exception("GridUtils : size should be > 1 !");

            double step = (end - start) / (size - 1);
            var grid = new double[size];
            for (int i = 0; i < size - 1; i++)
            {
                grid[i] = start + i * step;
            }
            grid[size - 1] = end;
            return grid;
        }

        /// <summary>
        /// Build a regular grid with endpoints {start, end}.
        /// </summary>
        /// <param name="interval">grid support</param>
        /// <param name="size">grid size (should be > 1) </param>
        /// <returns></returns>
        public static double[] RegularGrid(RealInterval interval, int size)
        {
            return RegularGrid(interval.Inf, interval.Sup, size);
        }
    }
}