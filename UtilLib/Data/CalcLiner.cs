using System;

namespace UtilLibrary.Data
{
    // https://gist.github.com/NikolayIT/d86118a3a0cb3f5ed63d674a350d75f2
    public class CalcLiner
    {
        //https://blog.acronym.co.kr/506 데이터 분석 관련 참고
        // 예제 
        /*
            // 연도
            var xValues = new double[]
                  {
                                  1990, 1991, 1992, 1993, 1994, 1995, 1996, 1997, 1998, 1999, 2000, 2001, 2002, 2003, 2004,
                                  2005, 2006, 2007, 2008, 2009
                  };

            // 매출
            var yValues = new double[]
                          {
                                  8669269, 8595500, 8484900, 8459800, 8427400, 8384700, 8340900, 8283200, 8230400, 8190900,
                                  8149468, 7932984, 7845841, 7801273, 7761049, 7720000, 7679290, 7640238, 7606551,
                                  7563710
                          };

            double rSquared, intercept, slope;
            LinearRegression(xValues, yValues, out rSquared, out intercept, out slope);

            Console.WriteLine($"R-squared = {rSquared}");
            Console.WriteLine($"Intercept = {intercept}");
            Console.WriteLine($"Slope = {slope}");

            // 2017년도 예상 매출 공식
            var predictedValue = (slope * 2017) + intercept;
            Console.WriteLine($"Prediction for 2017: {predictedValue}");
        */
        /// <summary>
        /// Fits a line to a collection of (x,y) points.
        /// </summary>
        /// <param name="xVals">The x-axis values.</param>
        /// <param name="yVals">The y-axis values.</param>
        /// <param name="rSquared">The r^2 value of the line.</param>
        /// <param name="yIntercept">The y-intercept value of the line (i.e. y = ax + b, yIntercept is b).</param>
        /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
        /// 
        public static void LinearRegression(
            double[] xVals,
            double[] yVals,
            out double rSquared,
            out double yIntercept,
            out double slope)
        {
            if (xVals.Length != yVals.Length) {
                throw new Exception("Input values should be with the same length.");
            }

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (var i = 0; i < xVals.Length; i++) {
                var x = xVals[i];
                var y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            var count = xVals.Length;
            var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            var meanX = sumOfX / count;
            var meanY = sumOfY / count;
            var dblR = rNumerator / Math.Sqrt(rDenom);

            rSquared = dblR * dblR * 100;
            yIntercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
    }
}
