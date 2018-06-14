using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using AWSLambda1.Extensions;

namespace AWSLambda1.Geo
{
    public struct GeoLocation : IEquatable<GeoLocation>, IFormattable
    {
        public static readonly GeoLocation NaN = new GeoLocation(double.NaN, double.NaN);
        public static readonly GeoLocation Zero = new GeoLocation(0.0, 0.0);
        public static readonly Distance RadiusEarth = Distance.FromKilometres(6371.0);
        private readonly double latitude;
        private readonly double longitude;
        private readonly double longitudeOffset;
        private const double OneMetreInDegrees = 1E-05;

        public GeoLocation(double latitude, double longitude)
        {
            if (latitude > 90.0 || latitude < -90.0)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Must be between +/- 90 (inclusive), not: " + (object)latitude);
            this.latitude = latitude;
            this.longitude = longitude;
            this.longitudeOffset = 0.0;
        }

        public GeoLocation Round(int precision)
        {
            return new GeoLocation(Math.Round(this.latitude, precision), Math.Round(this.longitude, precision));
        }

        public GeoLocation(Angle latitude, Angle longitude)
        {
            this = new GeoLocation(latitude.Degrees, longitude.Degrees);
        }

        public double Latitude
        {
            get
            {
                return this.latitude;
            }
           
        }

        public double Longitude
        {
            get
            {
                return this.longitude;
            }
           
        }

        
        public double LatitudeRadians
        {
            get
            {
                return Angle.DegreesToRadians(this.latitude);
            }
        }

        public double LongitudeRadians
        {
            get
            {
                return Angle.DegreesToRadians(this.longitude);
            }
        }

        public Angle LongitudeAngle
        {
            get
            {
                return Angle.FromDegrees(this.longitude);
            }
        }

        public Angle LatitudeAngle
        {
            get
            {
                return Angle.FromDegrees(this.latitude);
            }
        }

        public bool IsNormal
        {
            get
            {
                if (this.longitude <= 180.0)
                    return this.longitude >= -180.0;
                return false;
            }
        }

        public bool IsNaN
        {
            get
            {
                if (!double.IsNaN(this.Latitude))
                    return double.IsNaN(this.Longitude);
                return true;
            }
        }

        public double this[int index]
        {
            get
            {
                if (index == 0)
                    return this.Latitude;
                if (index == 1)
                    return this.Longitude;
                throw new ArgumentOutOfRangeException(nameof(index), $"Must be either 0 or 1, not {(object)index}.");
            }
        }

     

        public override string ToString()
        {
            return string.Format("{0} {1}", (object)this.latitude, (object)this.longitude);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            double num = this.latitude;
            string str1 = num.ToString(format, formatProvider);
            string str2 = " ";
            num = this.longitude;
            string str3 = num.ToString(format, formatProvider);
            return str1 + str2 + str3;
        }

        public bool Equals(GeoLocation other)
        {
            if (Math.Abs(other.latitude - this.latitude) < 1E-06)
                return Math.Abs(other.longitude - this.longitude) < 1E-06;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is GeoLocation)
                return this.Equals((GeoLocation)obj);
            return false;
        }

        public override int GetHashCode()
        {
            double num1 = this.latitude;
            int num2 = num1.GetHashCode() * 397;
            num1 = this.longitude;
            int hashCode = num1.GetHashCode();
            return num2 ^ hashCode;
        }

        public static bool operator ==(GeoLocation left, GeoLocation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GeoLocation left, GeoLocation right)
        {
            return !left.Equals(right);
        }

        public static GeoLocation operator +(GeoLocation left, GeoLocation right)
        {
            return new GeoLocation(left.Latitude + right.Latitude, left.Longitude + right.Longitude);
        }

        public static GeoLocation operator -(GeoLocation left, GeoLocation right)
        {
            return new GeoLocation(left.Latitude - right.Latitude, left.Longitude - right.Longitude);
        }

        public static bool TryParse(string s, out GeoLocation location)
        {
            Match match1 = Regex.Match(s, String.Format("^\\s*([0-9.-]+){0}([0-9.-]+)\\s*$", (object)"[\\s;,]+"));
            double res1;
            double res2;
            if (match1.Success && match1.Groups[1].Value.GetDoublePub(out res1) && (match1.Groups[2].Value.GetDoublePub(out res2) && res1 <= 90.0) && res1 >= -90.0)
            {
                location = new GeoLocation(res1, res2);
                return true;
            }
            Match match2 = Regex.Match(s, String.Format("^\\s*([0-9.-]+)[\\s°º]*([NS]?)[\\s°º]*{0}([0-9.-]+)[\\s°º]*([EW]?)[\\s°º]*$", (object)"[\\s;,]+"));
            double res3;
            double res4;
            if (match2.Success && match2.Groups[1].Value.GetDoublePub(out res3) && match2.Groups[3].Value.GetDoublePub(out res4))
            {
                if (match2.Groups[2].Value == "S")
                    res3 *= -1.0;
                if (match2.Groups[4].Value == "W")
                    res4 *= -1.0;
                if (res3 <= 90.0 && res3 >= -90.0)
                {
                    location = new GeoLocation(res3, res4);
                    return true;
                }
            }
            Match match3 = Regex.Match(s, String.Format("^\\s*([NS-]?)\\s*(\\d+)[º°\\'\\s-]+([0-9.]+)[\\s\\'′’´]?\\s*([NS]?){0}([EW-]?)\\s*(\\d+)[º°\\'\\s-]+([0-9.]+)[\\s\\'′’´]?\\s*([EW]?)\\s*$", (object)"[\\s;,]+"));
            double res5;
            double res6;
            double res7;
            double res8;
            if (match3.Success && match3.Groups[2].Value.GetDoublePub(out res5) && (match3.Groups[3].Value.GetDoublePub(out res6) && match3.Groups[6].Value.GetDoublePub(out res7)) && match3.Groups[7].Value.GetDoublePub(out res8))
            {
                double latitude = res5 + res6 / 60.0;
                double longitude = res7 + res8 / 60.0;
                if (match3.Groups[1].Value == "-" || match3.Groups[1].Value == "S" || match3.Groups[4].Value == "S")
                    latitude *= -1.0;
                if (match3.Groups[5].Value == "-" || match3.Groups[5].Value == "W" || match3.Groups[8].Value == "W")
                    longitude *= -1.0;
                if (latitude <= 90.0 && latitude >= -90.0)
                {
                    location = new GeoLocation(latitude, longitude);
                    return true;
                }
            }
            Match match4 = Regex.Match(s, String.Format("^\\s*([NS-]?)\\s*(\\d+)[º°\\s-]+(\\d+)[\\'’′´\\s]+([0-9.]+)[\"”″\\s]?\\s*([NS]?){0}([EW-]?)\\s*(\\d+)[º°\\s-]+(\\d+)[\\'’′´\\s]+([0-9.]+)[\"”″\\s]?\\s*([EW]?)\\s*$", (object)"[\\s;,]+"));
            double res9;
            double res10;
            double res11;
            double res12;
            double res13;
            double res14;
            if (match4.Success && match4.Groups[2].Value.GetDoublePub(out res9) && (match4.Groups[3].Value.GetDoublePub(out res10) && match4.Groups[4].Value.GetDoublePub(out res11)) && (match4.Groups[7].Value.GetDoublePub(out res12) && match4.Groups[8].Value.GetDoublePub(out res13) && match4.Groups[9].Value.GetDoublePub(out res14)))
            {
                double latitude = res9 + res10 / 60.0 + res11 / 3600.0;
                double longitude = res12 + res13 / 60.0 + res14 / 3600.0;
                if (match4.Groups[1].Value == "-" || match4.Groups[1].Value == "S" || match4.Groups[5].Value == "S")
                    latitude *= -1.0;
                if (match4.Groups[6].Value == "-" || match4.Groups[6].Value == "W" || match4.Groups[10].Value == "W")
                    longitude *= -1.0;
                if (latitude <= 90.0 && latitude >= -90.0)
                {
                    location = new GeoLocation(latitude, longitude);
                    return true;
                }
            }
            location = GeoLocation.NaN;
            return false;
        }

        public static GeoLocation Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            GeoLocation location;
            if (!GeoLocation.TryParse(s, out location))
                throw new FormatException("Unable to parse string as a GeoLocation: " + s);
            return location;
        }

        private static GeoLocation FromRadians(double latitudeRadians, double longitudeRadians)
        {
            return new GeoLocation(Angle.RadiansToDegrees(latitudeRadians), Angle.RadiansToDegrees(longitudeRadians));
        }

        public static Distance DistanceBetween(GeoLocation loc1, GeoLocation loc2)
        {
            return GeoLocation.DistanceBetween(loc1, loc2.Latitude, loc2.Longitude);
        }

        public static double EuclideanPerpendicularDistance(GeoLocation point1, GeoLocation point2, GeoLocation point)
        {
            double num1 = GeoLocation.DistanceBetweenTwoPointsWithPythagoras(point2, point);
            if (Math.Abs(num1) < 1E-05)
                return 0.0;
            double num2 = GeoLocation.DistanceBetweenTwoPointsWithPythagoras(point, point1);
            if (Math.Abs(num2) < 1E-05)
                return 0.0;
            double num3 = GeoLocation.DistanceBetweenTwoPointsWithPythagoras(point1, point2);
            if (Math.Abs(num3) < 1E-05)
                return num1;
            double num4 = (num3 + num1 + num2) / 2.0;
            if (Math.Abs(num4 - num1) < 1E-05)
                return num2;
            if (Math.Abs(num4 - num2) < 1E-05)
                return num1;
            return 2.0 * Math.Sqrt(num4 * (num4 - num3) * (num4 - num1) * (num4 - num2)) / num3;
        }

        public static bool EuclideanOnSegment(GeoLocation p, GeoLocation q, GeoLocation r)
        {
            return q.Longitude <= Math.Max(p.Longitude, r.Longitude) && q.Longitude >= Math.Min(p.Longitude, r.Longitude) && (q.Latitude <= Math.Max(p.Latitude, r.Latitude) && q.Latitude >= Math.Min(p.Latitude, r.Latitude));
        }

        public static int EuclideanOrientation(GeoLocation p, GeoLocation q, GeoLocation r)
        {
            double num = (q.Latitude - p.Latitude) * (r.Longitude - q.Longitude) - (q.Longitude - p.Longitude) * (r.Latitude - q.Latitude);
            if (Math.Abs(num) < 1E-15)
                return 0;
            return num <= 0.0 ? 2 : 1;
        }

        public static bool EuclideanLineIntersect(GeoLocation p1, GeoLocation q1, GeoLocation p2, GeoLocation q2)
        {
            int num1 = GeoLocation.EuclideanOrientation(p1, q1, p2);
            int num2 = GeoLocation.EuclideanOrientation(p1, q1, q2);
            int num3 = GeoLocation.EuclideanOrientation(p2, q2, p1);
            int num4 = GeoLocation.EuclideanOrientation(p2, q2, q1);
            return num1 != num2 && num3 != num4 || num1 == 0 && GeoLocation.EuclideanOnSegment(p1, p2, q1) || (num2 == 0 && GeoLocation.EuclideanOnSegment(p1, q2, q1) || num3 == 0 && GeoLocation.EuclideanOnSegment(p2, p1, q2)) || num4 == 0 && GeoLocation.EuclideanOnSegment(p2, q1, q2);
        }

        public static double DistanceBetweenTwoPointsWithPythagoras(GeoLocation point1, GeoLocation point2)
        {
            return Math.Sqrt(Math.Pow(point1.Longitude - point2.Longitude, 2.0) + Math.Pow(point1.Latitude - point2.Latitude, 2.0));
        }

        public static Distance DistanceBetween(GeoLocation loc1, double latitude, double longitude)
        {
            double latitude1 = loc1.Latitude;
            double radians1 = Angle.DegreesToRadians(latitude - latitude1);
            double radians2 = Angle.DegreesToRadians(longitude - loc1.Longitude);
            double num1 = 2.0;
            double num2 = Math.Sin(radians1 / num1);
            double num3 = Math.Sin(radians2 / 2.0);
            double d = num2 * num2 + Math.Cos(Angle.DegreesToRadians(latitude1)) * Math.Cos(Angle.DegreesToRadians(latitude)) * num3 * num3;
            double num4 = 2.0 * Math.Atan2(Math.Sqrt(d), Math.Sqrt(1.0 - d));
            return GeoLocation.RadiusEarth * num4;
        }

        public static GeoLocation FindPointAtDistanceFrom(GeoLocation startPoint, Angle initialBearing, Distance distance)
        {
            double num1 = distance.Kilometres / GeoLocation.RadiusEarth.Kilometres;
            double num2 = Math.Sin(num1);
            double num3 = Math.Cos(num1);
            double latitudeRadians = startPoint.LatitudeRadians;
            double longitudeRadians1 = startPoint.LongitudeRadians;
            double num4 = Math.Cos(latitudeRadians);
            double num5 = Math.Sin(latitudeRadians);
            double num6 = Math.Asin(num5 * num3 + num4 * num2 * initialBearing.Cos);
            double longitudeRadians2 = longitudeRadians1 + Math.Atan2(initialBearing.Sin * num2 * num4, num3 - num5 * Math.Sin(num6));
            return (GeoLocation)GeoLocation.FromRadians(num6, longitudeRadians2).Normalised();
        }

        public GeoLocation Normalised()
        {
            double longitude = this.longitude;
            while (longitude > 180.0)
                longitude -= 360.0;
            while (longitude < -180.0)
                longitude += 360.0;
            return (GeoLocation)new GeoLocation(this.latitude, longitude);
        }

        public static double FindLatitudeAtDistanceFrom(double latitude, Angle initialBearing, Distance distance)
        {
            double num1 = distance.Kilometres / GeoLocation.RadiusEarth.Kilometres;
            double num2 = Math.Sin(num1);
            double num3 = Math.Cos(num1);
            double radians = Angle.DegreesToRadians(latitude);
            return Angle.RadiansToDegrees(Math.Asin(Math.Sin(radians) * num3 + Math.Cos(radians) * num2 * initialBearing.Cos));
        }

        public static GeoLocation GreatCircleMidpoint(GeoLocation loc1, GeoLocation loc2)
        {
            double radians1 = Angle.DegreesToRadians(loc2.Longitude - loc1.Longitude);
            double radians2 = Angle.DegreesToRadians(loc1.Latitude);
            double radians3 = Angle.DegreesToRadians(loc2.Latitude);
            double num = Math.Cos(radians3) * Math.Cos(radians1);
            double y = Math.Cos(radians3) * Math.Sin(radians1);
            return new GeoLocation(Angle.RadiansToDegrees(Math.Atan2(Math.Sin(radians2) + Math.Sin(radians3), Math.Sqrt((Math.Cos(radians2) + num) * (Math.Cos(radians2) + num) + y * y))), Angle.RadiansToDegrees((Angle.DegreesToRadians(loc1.Longitude) + Math.Atan2(y, Math.Cos(radians2) + num) + 3.0 * Math.PI) % (2.0 * Math.PI) - Math.PI));
        }

        public static GeoLocation LoxodromicMidpoint(GeoLocation loc1, GeoLocation loc2)
        {
            Angle longitudeAngle1 = loc1.LongitudeAngle;
            Angle longitudeAngle2 = loc2.LongitudeAngle;
            Angle latitudeAngle1 = loc1.LatitudeAngle;
            Angle latitudeAngle2 = loc2.LatitudeAngle;
            Angle angle1 = longitudeAngle2 - longitudeAngle1;
            if (Math.Abs(angle1.Degrees) <= 180.0)
                return new GeoLocation((latitudeAngle1 + latitudeAngle2) / 2.0, (longitudeAngle1 + longitudeAngle2) / 2.0);
            Angle angle2 = angle1.Degrees < 0.0 ? Angle.TwoPi + angle1 : angle1 - Angle.TwoPi;
            return (GeoLocation)new GeoLocation((latitudeAngle1 + latitudeAngle2) / 2.0, longitudeAngle1 + angle2 / 2.0).Normalised();
        }

        public static Angle RhumbBearing(GeoLocation loc1, GeoLocation loc2)
        {
            Angle angle = loc2.LongitudeAngle - loc1.LongitudeAngle;
            if (Math.Abs(angle.Degrees) > 180.0)
                angle = angle.Degrees < 0.0 ? Angle.TwoPi + angle : angle - Angle.TwoPi;
            double x = Math.Log(Math.Tan(loc2.LatitudeRadians / 2.0 + Math.PI / 4.0) / Math.Tan(loc1.LatitudeRadians / 2.0 + Math.PI / 4.0));
            return Angle.FromRadians(Math.Atan2(angle.Radians, x));
        }

       
    }

    [DebuggerDisplay("{Degrees} deg")]
    public struct Angle : IEquatable<Angle>
    {
        public static readonly Angle Zero = new Angle(0.0);
        public static readonly Angle North = new Angle(0.0);
        public static readonly Angle South = new Angle(Math.PI);
        public static readonly Angle East = new Angle(Math.PI / 2.0);
        public static readonly Angle West = new Angle(3.0 * Math.PI / 2.0);
        public static readonly Angle TwoPi = new Angle(2.0 * Math.PI);
        public static readonly Angle Pi = new Angle(Math.PI);
        public static readonly Angle NaN = new Angle(double.NaN);
        private const double Epsilon = 0.0001;

        public static Angle FromRadians(double radians)
        {
            return new Angle(radians);
        }

        public static Angle FromDegrees(double degrees)
        {
            return new Angle(Angle.DegreesToRadians(degrees));
        }

        private Angle(double radians)
        {
            this = new Angle();
            this.Radians = radians;
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        public double Radians { get; private set; }

        public double Degrees
        {
            get
            {
                return Angle.RadiansToDegrees(this.Radians);
            }
        }

        public double Cos
        {
            get
            {
                return Math.Cos(this.Radians);
            }
        }

        public double Sin
        {
            get
            {
                return Math.Sin(this.Radians);
            }
        }

        public double Tan
        {
            get
            {
                return Math.Tan(this.Radians);
            }
        }

        public bool IsNaN
        {
            get
            {
                return double.IsNaN(this.Radians);
            }
        }

        public Angle Abs
        {
            get
            {
                return new Angle(Math.Abs(this.Radians));
            }
        }

        public Angle Normalise()
        {
            double radians = this.Radians;
            while (radians < 0.0)
                radians += 2.0 * Math.PI;
            if (radians >= 2.0 * Math.PI)
                radians %= 2.0 * Math.PI;
            return Angle.FromRadians(radians);
        }

        public Angle Limit(Angle lowerLimit, Angle upperLimit)
        {
            if (lowerLimit > upperLimit)
                throw new ArgumentException("The lower limit must be less than the upper limit.");
            if (this < lowerLimit)
                return lowerLimit;
            if (this > upperLimit)
                return upperLimit;
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is Angle)
                return this.Equals((Angle)obj);
            return false;
        }

        public bool Equals(Angle other)
        {
            if (!double.IsNaN(this.Radians) || !double.IsNaN(other.Radians))
                return Math.Abs(other.Radians - this.Radians) < 0.0001;
            return true;
        }

        public override int GetHashCode()
        {
            return this.Radians.GetHashCode();
        }

        public static Angle operator +(Angle a, Angle b)
        {
            return Angle.FromRadians(a.Radians + b.Radians);
        }

        public static Angle operator -(Angle a, Angle b)
        {
            return Angle.FromRadians(a.Radians - b.Radians);
        }

        public static Angle operator -(Angle a)
        {
            return Angle.FromRadians(-a.Radians);
        }

        public static Angle operator *(Angle a, double scale)
        {
            return Angle.FromRadians(a.Radians * scale);
        }

        public static Angle operator /(Angle a, double quotient)
        {
            return Angle.FromRadians(a.Radians / quotient);
        }

        public static bool operator >(Angle left, Angle right)
        {
            return left.Radians > right.Radians;
        }

        public static bool operator <(Angle left, Angle right)
        {
            return left.Radians < right.Radians;
        }

        public static bool operator >=(Angle left, Angle right)
        {
            return left.Radians >= right.Radians;
        }

        public static bool operator <=(Angle left, Angle right)
        {
            return left.Radians <= right.Radians;
        }

        public static bool operator ==(Angle left, Angle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Angle left, Angle right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("{0:0.##} degrees", (object)this.Degrees);
        }
    }
}