using System;

namespace Moserware.Numerics
{
    public class GaussianDistribution
    {
        // Intentionally, we're not going to derive related things, but set them all at once
        // to get around some NaN issues

        private GaussianDistribution()
        {
        }

        public GaussianDistribution(double mean, double standardDeviation)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
            Variance = Square(StandardDeviation);
            Precision = 1.0/Variance;
            PrecisionMean = Precision*Mean;
        }

        public double Mean { get; private set; }
        public double StandardDeviation { get; private set; }

        // Precision and PrecisionMean are used because they make multiplying and dividing simpler
        // (the the accompanying math paper for more details)

        public double Precision { get; private set; }

        public double PrecisionMean { get; private set; }

        private double Variance { get; set; }

        public double NormalizationConstant
        {
            get
            {
                // Great derivation of this is at http://www.astro.psu.edu/~mce/A451_2/A451/downloads/notes0.pdf
                return 1.0/(Math.Sqrt(2*Math.PI)*StandardDeviation);
            }
        }

        public GaussianDistribution Clone()
        {
            var result = new GaussianDistribution();
            result.Mean = Mean;
            result.StandardDeviation = StandardDeviation;
            result.Variance = Variance;
            result.Precision = Precision;
            result.PrecisionMean = PrecisionMean;
            return result;
        }

        public static GaussianDistribution FromPrecisionMean(double precisionMean, double precision)
        {
            var gaussianDistribution = new GaussianDistribution();
            gaussianDistribution.Precision = precision;
            gaussianDistribution.PrecisionMean = precisionMean;
            gaussianDistribution.Variance = 1.0/precision;
            gaussianDistribution.StandardDeviation = Math.Sqrt(gaussianDistribution.Variance);
            gaussianDistribution.Mean = gaussianDistribution.PrecisionMean/gaussianDistribution.Precision;
            return gaussianDistribution;
        }

        // Although we could use equations from // For details, see http://www.tina-vision.net/tina-knoppix/tina-memo/2003-003.pdf
        // for multiplication, the precision mean ones are easier to write :)
        public static GaussianDistribution operator *(GaussianDistribution left, GaussianDistribution right)
        {
            return FromPrecisionMean(left.PrecisionMean + right.PrecisionMean, left.Precision + right.Precision);
        }

        /// Computes the absolute difference between two Gaussians
        public static double AbsoluteDifference(GaussianDistribution left, GaussianDistribution right)
        {
            return Math.Max(
                Math.Abs(left.PrecisionMean - right.PrecisionMean),
                Math.Sqrt(Math.Abs(left.Precision - right.Precision)));
        }

        /// Computes the absolute difference between two Gaussians
        public static double operator -(GaussianDistribution left, GaussianDistribution right)
        {
            return AbsoluteDifference(left, right);
        }

        public static double LogProductNormalization(GaussianDistribution left, GaussianDistribution right)
        {
            if ((left.Precision == 0) || (right.Precision == 0))
            {
                return 0;
            }

            double varianceSum = left.Variance + right.Variance;
            double meanDifference = left.Mean - right.Mean;

            double logSqrt2Pi = Math.Log(Math.Sqrt(2*Math.PI));
            return -logSqrt2Pi - (Math.Log(varianceSum)/2.0) - (Square(meanDifference)/(2.0*varianceSum));
        }


        public static GaussianDistribution operator /(GaussianDistribution numerator, GaussianDistribution denominator)
        {
            return FromPrecisionMean(numerator.PrecisionMean - denominator.PrecisionMean,
                                     numerator.Precision - denominator.Precision);
        }

        public static double LogRatioNormalization(GaussianDistribution numerator, GaussianDistribution denominator)
        {
            if ((numerator.Precision == 0) || (denominator.Precision == 0))
            {
                return 0;
            }

            double varianceDifference = denominator.Variance - numerator.Variance;
            double meanDifference = numerator.Mean - denominator.Mean;

            double logSqrt2Pi = Math.Log(Math.Sqrt(2*Math.PI));

            return Math.Log(denominator.Variance) + logSqrt2Pi - Math.Log(varianceDifference)/2.0 +
                   Square(meanDifference)/(2*varianceDifference);
        }

        private static double Square(double x)
        {
            return x*x;
        }

        public static double At(double x)
        {
            return At(x, 0, 1);
        }

        public static double At(double x, double mean, double standardDeviation)
        {
            // See http://mathworld.wolfram.com/NormalDistribution.html
            //                1              -(x-mean)^2 / (2*stdDev^2)
            // P(x) = ------------------- * e
            //        stdDev * sqrt(2*pi)

            double multiplier = 1.0/(standardDeviation*Math.Sqrt(2*Math.PI));
            double expPart = Math.Exp((-1.0*Math.Pow(x - mean, 2.0))/(2*(standardDeviation*standardDeviation)));
            double result = multiplier*expPart;
            return result;
        }

        public static double CumulativeTo(double x, double mean, double standardDeviation)
        {
            double invsqrt2 = -0.707106781186547524400844362104;
            double result = ErrorFunctionCumulativeTo(invsqrt2*x);
            return 0.5*result;
        }

        public static double CumulativeTo(double x)
        {
            return CumulativeTo(x, 0, 1);
        }

        private static double ErrorFunctionCumulativeTo(double x)
        {
            // Derived from page 265 of Numerical Recipes 3rd Edition            
            double z = Math.Abs(x);

            double t = 2.0/(2.0 + z);
            double ty = 4*t - 2;

            double[] coefficients = {
                                        -1.3026537197817094, 6.4196979235649026e-1,
                                        1.9476473204185836e-2, -9.561514786808631e-3, -9.46595344482036e-4,
                                        3.66839497852761e-4, 4.2523324806907e-5, -2.0278578112534e-5,
                                        -1.624290004647e-6, 1.303655835580e-6, 1.5626441722e-8, -8.5238095915e-8,
                                        6.529054439e-9, 5.059343495e-9, -9.91364156e-10, -2.27365122e-10,
                                        9.6467911e-11, 2.394038e-12, -6.886027e-12, 8.94487e-13, 3.13092e-13,
                                        -1.12708e-13, 3.81e-16, 7.106e-15, -1.523e-15, -9.4e-17, 1.21e-16, -2.8e-17
                                    };

            int ncof = coefficients.Length;
            double d = 0.0;
            double dd = 0.0;


            for (int j = ncof - 1; j > 0; j--)
            {
                double tmp = d;
                d = ty*d - dd + coefficients[j];
                dd = tmp;
            }

            double ans = t*Math.Exp(-z*z + 0.5*(coefficients[0] + ty*d) - dd);
            return x >= 0.0 ? ans : (2.0 - ans);
        }


        private static double InverseErrorFunctionCumulativeTo(double p)
        {
            // From page 265 of numerical recipes                       

            if (p >= 2.0)
            {
                return -100;
            }
            if (p <= 0.0)
            {
                return 100;
            }

            double pp = (p < 1.0) ? p : 2 - p;
            double t = Math.Sqrt(-2*Math.Log(pp/2.0)); // Initial guess
            double x = -0.70711*((2.30753 + t*0.27061)/(1.0 + t*(0.99229 + t*0.04481)) - t);

            for (int j = 0; j < 2; j++)
            {
                double err = ErrorFunctionCumulativeTo(x) - pp;
                x += err/(1.12837916709551257*Math.Exp(-(x*x)) - x*err); // Halley                
            }

            return p < 1.0 ? x : -x;
        }

        public static double InverseCumulativeTo(double x, double mean, double standardDeviation)
        {
            // From numerical recipes, page 320
            return mean - Math.Sqrt(2)*standardDeviation*InverseErrorFunctionCumulativeTo(2*x);
        }

        public static double InverseCumulativeTo(double x)
        {
            return InverseCumulativeTo(x, 0, 1);
        }


        public override string ToString()
        {
            // Debug help
            return String.Format("μ={0:0.0000}, σ={1:0.0000}",
                                 Mean,
                                 StandardDeviation);
        }
    }
}