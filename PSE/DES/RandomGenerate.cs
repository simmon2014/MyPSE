using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSE.DES
{
    /// <summary>
    /// This class handles issues related with random number generation.
    /// @author Arda Ceylan
    /// </summary>
    public class RandomGenerate
    {
        /// <summary>
        /// Is used to generate random numbers.
        /// </summary>
        static public Random rnd;


        /// <summary>
        /// Version 1.2 note: Is added to prevent faulty user entry.
        /// @author Emre VAROL
        /// </summary>
        public enum dist
        {
            /// <summary>
            /// no distribution
            /// </summary>
            none,
            /// <summary>
            /// Exponential Distribution
            /// </summary>
            exponential,
            /// <summary>
            /// Normal distribution
            /// </summary>
            normal,
            /// <summary>
            /// Uniform distribution
            /// </summary>
            uniform,
            /// <summary>
            /// Gamma distribution
            /// </summary>
            gamma,
            /// <summary>
            /// ChiSquare distribution
            /// </summary>
            chiSquare,
            /// <summary>
            /// InverseGamma distribution
            /// </summary>
            inverseGamma,
            /// <summary>
            /// Weibull distribution
            /// </summary>
            weibull,
            /// <summary>
            /// Cauchy distribution
            /// </summary>
            cauchy,
            /// <summary>
            /// StudenT distribution
            /// </summary>
            studentT,
            /// <summary>
            /// Laplace distribution
            /// </summary>
            laplace,
            /// <summary>
            /// LogNormal distribution
            /// </summary>
            logNormal,
            /// <summary>
            /// Beta distribution
            /// </summary>
            beta,
        }

        /// <summary>
        /// Constructs a new RandomGenerate.
        /// </summary>
        public RandomGenerate()
        {
            rnd = new Random();
        }

        /// <summary>
        /// Constructs a new RandomGenerate.
        /// </summary>
        /// <param name="seed"> Is used as a seed for Random type variable. </param>
        public RandomGenerate(int seed)
        {
            rnd = new Random(seed);
        }
        /// <summary>
        /// Version 1.2 note: Generate random double numbers between min and max
        /// @author Emre VAROL
        /// </summary>
        /// <param name="minimum">Minimum value</param>
        /// <param name="maximum">Maximum value</param>
        /// <returns>Double</returns>
        public static double GetRandomDoubleNumber(double minimum, double maximum)
        {
            return rnd.NextDouble() * (maximum - minimum) + minimum;
        }
        /// <summary> 
        /// Version 1.1 note: In this version overloaded method for ComputeValue is created.
        /// Compute Value takes a List in which the first element is distribution name.
        /// @author Emre VAROL
        /// </summary>
        /// <param name="simDist"></param>
        /// <returns></returns>
        public static double ComputeValue(List<object> simDist)
        {
            double value = 0;
            string distName = (string)simDist[0];
            int numberParams = simDist.Count() - 1;
            switch (numberParams)
            {
                case 1:
                    {
                        value = ComputeValue(distName, (double) simDist[numberParams], 0.0);
                        break;
                    }
                case 2:
                    {
                        value = ComputeValue(distName, (double)simDist[numberParams - 1], (double)simDist[numberParams]);
                        break;
                    }
                default:
                    break;
            }
            return value;
        }


        /// <summary>
        /// Version 1.1 note: In this version overloaded method for ComputeValue and more distributions are created.
        /// Generates a random number according with given arguments.
        /// @author Emre VAROL
        /// </summary>
        /// <param name="dist"> The name of the distribution which is used to compute the time passed on that arc. </param>
        /// <param name="param1"> First parameter of the distribution. </param>
        /// <param name="param2"> Second parameter of the distribution. (if necessary) </param>
        /// <returns></returns>
        public static double ComputeValue(string dist, double param1, double param2)
        {
            double value = 0;
            switch (dist)
            {
                case "exponential":
                    {
                        value = GetExponential(param1);
                        break;
                    }
                case "normal":
                    {
                        value = GetNormal(param1, param2);
                        break;
                    }
                case "uniform":
                    {
                        value = GetUniform(param1, param2);
                        break;
                    }
                case "gamma":
                    {
                        value = GetGamma(param1, param2);
                        break;
                    }
                case "chiSquare":
                    {
                        value = GetChiSquare(param1);
                        break;
                    }
                case "inverseGamma":
                    {
                        value = GetInverseGamma(param1, param2);
                        break;
                    }
                case "weibull":
                    {
                        value = GetWeibull(param1, param2);
                        break;
                    }
                case "cauchy":
                    {
                        value = GetCauchy(param1, param2);
                        break;
                    }
                case "studentT":
                    {
                        value = GetStudentT(param1);
                        break;
                    }
                case "laplace":
                    {
                        value = GetLaplace(param1, param2);
                        break;
                    }
                case "logNormal":
                    {
                        value = GetLogNormal(param1, param2);
                        break;
                    }
                case "beta":
                    {
                        value = GetBeta(param1, param2);
                        break;
                    }
                default:
                    break;
            }
            return value;
        }

        /// <summary>
        /// Version 1.2 note: In this version overloaded method for ComputeValue and more distributions are created.
        /// Generates a random number according with given arguments.
        /// @author Emre VAROL
        /// </summary>
        /// <param name="distribution">The enumeration of the distribution which is used to compute the time passed on that arc.</param>
        /// <param name="param1"> First parameter of the distribution. </param>
        /// <param name="param2"> Second parameter of the distribution. (if necessary) </param>
        /// <returns></returns>
        public static double ComputeValue(dist distribution, double param1, double param2)
        {
            double value = 0;
            switch (distribution)
            {
                case dist.exponential:
                    {
                        value = GetExponential(param1);
                        break;
                    }
                case dist.normal:
                    {
                        value = GetNormal(param1, param2);
                        break;
                    }
                case dist.uniform:
                    {
                        value = GetUniform(param1, param2);
                        break;
                    }
                case dist.gamma:
                    {
                        value = GetGamma(param1, param2);
                        break;
                    }
                case dist.chiSquare:
                    {
                        value = GetChiSquare(param1);
                        break;
                    }
                case dist.inverseGamma:
                    {
                        value = GetInverseGamma(param1, param2);
                        break;
                    }
                case dist.weibull:
                    {
                        value = GetWeibull(param1, param2);
                        break;
                    }
                case dist.cauchy:
                    {
                        value = GetCauchy(param1, param2);
                        break;
                    }
                case dist.studentT:
                    {
                        value = GetStudentT(param1);
                        break;
                    }
                case dist.laplace:
                    {
                        value = GetLaplace(param1, param2);
                        break;
                    }
                case dist.logNormal:
                    {
                        value = GetLogNormal(param1, param2);
                        break;
                    }
                case dist.beta:
                    {
                        value = GetBeta(param1, param2);
                        break;
                    }
                default:
                    break;
            }
            return value;
        }

        /// <summary>
        /// Generates a boolean value considering the success rate.
        /// </summary>
        /// <param name="criticalValue"></param>
        /// <returns></returns>
        public static bool GenerateBool(double criticalValue)
        {
            bool value = true;
            if (rnd.NextDouble() < criticalValue)
                value = false;
            return value;
        }

        /// <summary>
        /// Generates an integer value between 0 and provided value.
        /// </summary>
        /// <param name="criticalValue"></param>
        /// <returns></returns>
        public static int GenerateInteger(int criticalValue)
        {
            int value = 0;
            value = rnd.Next(criticalValue);
            return value;
        }
 
        /// <summary>
        /// Produce a uniform random sample from the open interval (0, 1).
        /// </summary>
        /// <returns></returns>
        public static double GetUniform()
        {
            return rnd.NextDouble();
        }

        /// <summary>
        /// Produce a random sample from the open interval (a, b).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double GetUniform(double a, double b)
        {
            return a + rnd.NextDouble() * (b - a);
        }

        /// <summary>
        /// Get normal (Gaussian) random sample with mean 0 and standard deviation 1
        /// </summary>
        /// <returns></returns>
        public static double GetNormal()
        {
            // Use Box-Muller algorithm
            double u1 = GetUniform();
            double u2 = GetUniform();
            double r = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;
            return r * Math.Sin(theta);
        }

        /// <summary>
        ///  Get normal (Gaussian) random sample with specified mean and standard deviation
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="standardDeviation"></param>
        /// <returns></returns>
        public static double GetNormal(double mean, double standardDeviation)
        {
            if (standardDeviation < 0.0)
            {
                string msg = string.Format("Standard Deviation must be positive. Received {Negative}.", standardDeviation);
                throw new ArgumentOutOfRangeException(msg);
            }
            return mean + standardDeviation*GetNormal();
        }

        /// <summary>
        /// Get exponential random sample with mean 1
        /// </summary>
        /// <returns></returns>
        public static double GetExponential()
        {
            return Math.Log(rnd.NextDouble());
        }

        /// <summary>
        /// Get exponential random sample with specified mean
        /// </summary>
        /// <param name="mean"></param>
        /// <returns></returns>
        public static double GetExponential(double mean)
        {
            return -mean*Math.Log(rnd.NextDouble());
        }

        /// <summary>
        /// Get gamma random sample with parameters
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double GetGamma(double shape, double scale)
        {
            // Implementation based on "A Simple Method for Generating Gamma Variables"
            // by George Marsaglia and Wai Wan Tsang.  ACM Transactions on Mathematical Software
            // Vol 26, No 3, September 2000, pages 363-372.

            double d, c, x, xsquared, v, u;

            if (shape >= 1.0)
            {
                d = shape - 1.0 / 3.0;
                c = 1.0 / Math.Sqrt(9.0 * d);
                for (;;)
                {
                    do
                    {
                        x = GetNormal();
                        v = 1.0 + c * x;
                    }
                    while (v <= 0.0);
                    v = v * v * v;
                    u = GetUniform();
                    xsquared = x * x;
                    if (u < 1.0 - .0331 * xsquared * xsquared || Math.Log(u) < 0.5 * xsquared + d * (1.0 - v + Math.Log(v)))
                        return scale * d * v;
                }
            }
            else if (shape <= 0.0)
            {
                string msg = string.Format("Shape must be positive. Received {0}.", shape);
                throw new ArgumentOutOfRangeException(msg);
            }
            else
            {
                double g = GetGamma(shape + 1.0, 1.0);
                double w = GetUniform();
                return scale * g * Math.Pow(w, 1.0 / shape);
            }
        }

        /// <summary>
        /// Get Chi Square random sample with parameter
        /// </summary>
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        public static double GetChiSquare(double degreesOfFreedom)
        {
            // A chi squared distribution with n degrees of freedom
            // is a gamma distribution with shape n/2 and scale 2.
            return GetGamma(0.5 * degreesOfFreedom, 2.0);
        }

        /// <summary>
        /// Get inverse gamma random sample with parameters
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double GetInverseGamma(double shape, double scale)
        {
            return 1.0 / GetGamma(shape, 1.0 / scale);
        }

        /// <summary>
        /// Get weibull random sample with parameters
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double GetWeibull(double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0)
            {
                string msg = string.Format("Shape and scale parameters must be positive. Recieved shape {0} and scale{1}.", shape, scale);
                throw new ArgumentOutOfRangeException(msg);
            }
            return scale * Math.Pow(-Math.Log(GetUniform()), 1.0 / shape);
        }

        /// <summary>
        /// Get cauchy random sample with parameters
        /// </summary>
        /// <param name="median"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double GetCauchy(double median, double scale)
        {
            if (scale <= 0)
            {
                string msg = string.Format("Scale must be positive. Received {0}.", scale);
                throw new ArgumentException(msg);
            }

            double p = GetUniform();

            // Apply inverse of the Cauchy distribution function to a uniform
            return median + scale * Math.Tan(Math.PI * (p - 0.5));
        }

        /// <summary>
        /// Get student T random sample with parameters
        /// </summary>
        /// <param name="degreesOfFreedom"></param>
        /// <returns></returns>
        public static double GetStudentT(double degreesOfFreedom)
        {
            if (degreesOfFreedom <= 0)
            {
                string msg = string.Format("Degrees of freedom must be positive. Received {0}.", degreesOfFreedom);
                throw new ArgumentException(msg);
            }

            // See Seminumerical Algorithms by Knuth
            double y1 = GetNormal();
            double y2 = GetChiSquare(degreesOfFreedom);
            return y1 / Math.Sqrt(y2 / degreesOfFreedom);
        }

        
        /// <summary>
        /// Get laplace random sample with parameters
        /// The Laplace distribution is also known as the double exponential distribution.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double GetLaplace(double mean, double scale)
        {
            double u = GetUniform();
            return (u < 0.5) ?
                mean + scale * Math.Log(2.0 * u) :
                mean - scale * Math.Log(2 * (1 - u));
        }

        /// <summary>
        /// Get LogNormal random sample with parameters
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double GetLogNormal(double mu, double sigma)
        {
            return Math.Exp(GetNormal(mu, sigma));
        }

        /// <summary>
        /// Get beta random sample with parameters
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double GetBeta(double a, double b)
        {
            if (a <= 0.0 || b <= 0.0)
            {
                string msg = string.Format("Beta parameters must be positive. Received {0} and {1}.", a, b);
                throw new ArgumentOutOfRangeException(msg);
            }
            // There are more efficient methods for generating beta samples.
            // However such methods are a little more efficient and much more complicated.
            // For an explanation of why the following method works, see
            // http://www.johndcook.com/distribution_chart.html#gamma_beta
            double u = GetGamma(a, 1.0);
            double v = GetGamma(b, 1.0);
            return u / (u + v);
        }
    }
}
