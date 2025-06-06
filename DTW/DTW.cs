﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Recorder.MFCC;

namespace Recorder
{
    static public class DTW
    {
        static public double EuclideanDistance(double[] a, double[] b)
        {
            double res = 0;
            int maxSize = Math.Max(a.Length, b.Length);
            int minSize = Math.Min(a.Length, b.Length);
            for (int i = 0; i < maxSize; i++)
            {
                if (i < minSize)
                {
                    double diff = a[i] - b[i];
                    res += diff * diff;
                }
                else
                    res += (a.Length > b.Length ? a[i] * a[i] : b[i] * b[i]);
            }

            return Math.Sqrt(res);
        }

        //static public double CalculateDTWDistanceWithWindow(Sequence a, Sequence b, double[][] distanceMatrix, int width)
        //{
        //    int n = a.Frames.Length, m = b.Frames.Length;
        //    double[][] dp = new double[n + 1][];
        //    for (int i = 0; i <= n; i++)
        //        dp[i] = new double[m + 1];

        //    for (int i = 0; i <= n; i++)
        //        for (int j = 0; j <= m; j++)
        //            dp[i][j] = double.PositiveInfinity;

        //    dp[0][0] = 0;
        //    for (int i = 1; i <= n; i++)
        //        for (int j = Math.Max(1, i - width); j <= Math.Min(m, i + width); j++)
        //        {
        //            double distancePrev = distanceMatrix[i - 1][j - 1];
        //            double shrinked = dp[i][j - 1], stretched = dp[i - 1][j], next = dp[i - 1][j - 1];
        //            dp[i][j] = Math.Min(Math.Min(shrinked, stretched), next) + distancePrev;
        //        }

        //    return dp[n][m];
        //}

        // limiting search paths
        static public double CalculateDTWDistanceWithWindow(Sequence a, Sequence b, int width)
        {
            int n = a.Frames.Length, m = b.Frames.Length;
            double[][] dp = new double[2][];
            int[][] lastUsed = new int[2][];
            for (int i = 0; i < 2; i++)
            {
                dp[i] = new double[m + 1];
                lastUsed[i] = new int[m + 1];
                for (int j = 0; j <= m; j++)
                {
                    lastUsed[i][j] = -1;
                }
            }

            dp[0][0] = 0;
            lastUsed[0][0] = 0;
            width = Math.Max(width, 2 * Math.Abs(n - m));
            for (int i = 1; i <= n; i++)
            {

                for (int j = Math.Max(1, i - width / 2); j <= Math.Min(m, i + width / 2); j++)
                {
                    double distancePrev = EuclideanDistance(a.Frames[i - 1].Features, b.Frames[j - 1].Features);
                    double next = (lastUsed[(i - 1) & 1][j - 1] == i - 1) ?
                        dp[(i - 1) & 1][j - 1] : double.PositiveInfinity,
                           shrinked = (j >= 2 && lastUsed[(i - 1) & 1][j - 2] == i - 1) ?
                        dp[(i - 1) & 1][j - 2] : double.PositiveInfinity,
                           stretched = (lastUsed[(i - 1) & 1][j] == i - 1) ?
                        dp[(i - 1) & 1][j] : double.PositiveInfinity;
                    dp[i & 1][j] = Math.Min(Math.Min(shrinked, stretched), next) + distancePrev;
                    lastUsed[i & 1][j] = i;
                }
            }
            return dp[n & 1][m];
        }

static public double CalculateDTWDistanceWithBeam(Sequence a, Sequence b, int width)
{
    int n = a.Frames.Length, m = b.Frames.Length;
    double[][] dp = new double[2][];
    bool[][] valid = new bool[2][]; // this array tells us whether a cell is pruned
    for (int i = 0; i < 2; i++)
    {
        dp[i] = new double[m + 1];
        valid[i] = new bool[m + 1];
    }

    for (int i = 0; i < 2; i++) // initializing all distances to infinity
        for (int j = 0; j <= m; j++)
            dp[i][j] = double.PositiveInfinity;

    dp[0][0] = 0; // base case
    valid[0][0] = true;
    for (int i = 1; i <= n; i++)
    {
        // reset current row
        for (int j = 0; j <= m; j++)
        {
            dp[i & 1][j] = double.PositiveInfinity;
            valid[i & 1][j] = false;
        }
        double bestCost = double.PositiveInfinity; // initally set to a large value
        for (int j = 1; j <= m; j++)
        {
            // if all of the predecessors are pruned: invalid transitions so we skip
            if (!valid[(i - 1) & 1][j] &&
                !valid[(i - 1) & 1][j - 1] &&
                j >= 2 && !valid[(i - 1) & 1][j - 2])
            {
                continue;
            }
            // distance between frames
            double distancePrev = EuclideanDistance(a.Frames[i - 1].Features, b.Frames[j - 1].Features);
            double next = dp[(i - 1) & 1][j - 1],
                    shrinked = (j >= 2 ? dp[(i - 1) & 1][j - 2] : double.PositiveInfinity),
                    stretched = dp[(i - 1) & 1][j];
            dp[i & 1][j] = Math.Min(Math.Min(shrinked, stretched), next) + distancePrev;
            bestCost = Math.Min(bestCost, dp[i & 1][j]);
        }
        for (int j = 1; j <= m; j++)
        {
            if (dp[i & 1][j] <= bestCost + width)
                valid[i & 1][j] = true;
            else
                dp[i & 1][j] = double.PositiveInfinity;
        }
    }
    return dp[n & 1][m];
}

        static public double[][] ConstructDistanceMatrix(int n, int m, Sequence a, Sequence b)
        {
            double[][] distanceMatrix = new double[n][];
            for (int i = 0; i < n; i++)
                distanceMatrix[i] = new double[m];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    distanceMatrix[i][j] = EuclideanDistance(a.Frames[i].Features, b.Frames[j].Features);
            return distanceMatrix;
        }

        //static public double DTWDistance(Sequence a, Sequence b, double[][] distanceMatrix)
        //{
        //    int n = a.Frames.Length, m = b.Frames.Length;
        //    double[][] dp = new double[n + 1][];
        //    for (int i = 0; i <= n; i++)
        //        dp[i] = new double[m + 1];

        //    for (int i = 0; i <= n; i++)
        //        for (int j = 0; j <= m; j++)
        //            dp[i][j] = double.PositiveInfinity;

        //    dp[0][0] = 0;
        //    for (int i = 1; i <= n; i++)
        //        for (int j = 1; j <= m; j++)
        //        {
        //            double distancePrev = distanceMatrix[i - 1][j - 1];
        //            double shrinked = dp[i][j - 1], stretched = dp[i - 1][j], next = dp[i - 1][j - 1];
        //            dp[i][j] = Math.Min(Math.Min(shrinked, stretched), next) + distancePrev;
        //        }

        //    return dp[n][m];
        //}

        //overloaded function that doesn't require separate initlization of distanceMatrix
        static public double DTWDistance(Sequence a, Sequence b)
        {

            //double[][] distanceMatrix = ConstructDistanceMatrix(a.Frames.Length, b.Frames.Length, a, b);

            int n = a.Frames.Length, m = b.Frames.Length;
            double[][] dp = new double[2][];
            for (int i = 0; i < 2; i++)
                dp[i] = new double[m + 1];

            for (int i = 0; i < 2; i++)
                for (int j = 0; j <= m; j++)
                    dp[i][j] = double.PositiveInfinity;

            dp[0][0] = 0;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 0; j <= m; j++)
                    dp[i & 1][j] = double.PositiveInfinity;
                for (int j = 1; j <= m; j++)
                {
                    double distancePrev = EuclideanDistance(a.Frames[i - 1].Features, b.Frames[j - 1].Features);
                    double next = dp[(i - 1) & 1][j - 1],
                           shrinked = (j >= 2 ? dp[(i - 1) & 1][j - 2] : double.PositiveInfinity),
                           stretched = dp[(i - 1) & 1][j];
                    dp[i & 1][j] = Math.Min(Math.Min(shrinked, stretched), next) + distancePrev;
                }
            }
            return dp[n & 1][m];
        }
    }
}
