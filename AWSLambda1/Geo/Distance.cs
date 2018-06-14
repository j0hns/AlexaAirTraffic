using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.Geo
{
    public struct Distance : IEquatable<Distance>
    {
        public const double MileToKm = 1.609344;
        public const double NauticalMileToKm = 1.852;

        public static readonly Distance Zero = new Distance(0);

        public static Distance FromKilometres(double kilometres)
        {
            return new Distance(kilometres);
        }

        public static Distance FromMetres(double radiusInMeters)
        {
            return new Distance(radiusInMeters / 1000.0);
        }

        public static Distance FromNauticalMiles(double nauticalMiles)
        {
            return new Distance(NauticalMilesToKilometres(nauticalMiles));
        }

        public static double NauticalMilesToKilometres(double nauticalMiles)
        {
            return nauticalMiles * NauticalMileToKm;
        }

        public static double KilometresToNauticalMiles(double kilometres)
        {
            return kilometres / NauticalMileToKm;
        }

        public static double MilesToKilometres(double miles)
        {
            return miles * MileToKm;
        }

        public static double KilometresToMiles(double kilometres)
        {
            return kilometres / MileToKm;
        }

        public double Kilometres { get; private set; }
        public double Metres { get { return Kilometres * 1000D; } }

        public double NauticalMiles
        {
            get { return KilometresToNauticalMiles(Kilometres); }
        }

        public double Miles
        {
            get { return KilometresToMiles(Kilometres); }
        }

        private Distance(double kilometres)
            : this()
        {
            if (kilometres < 0)
            {
                throw new ArgumentOutOfRangeException("kilometres", "Must be greater than or equal to zero.");
            }

            Kilometres = kilometres;
        }

        public bool Equals(Distance other)
        {
            return other.Kilometres.Equals(Kilometres);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (obj.GetType() != typeof(Distance))
            {
                return false;
            }

            return Equals((Distance)obj);
        }

        public override int GetHashCode()
        {
            return Kilometres.GetHashCode();
        }

        public static bool operator ==(Distance left, Distance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Distance left, Distance right)
        {
            return !left.Equals(right);
        }

        public static Distance operator +(Distance a, Distance b)
        {
            return FromKilometres(a.Kilometres + b.Kilometres);
        }

        public static Distance operator -(Distance a, Distance b)
        {
            return FromKilometres(a.Kilometres - b.Kilometres);
        }

        public static Distance operator -(Distance a)
        {
            return FromKilometres(-a.Kilometres);
        }

        public static Distance operator *(Distance a, double scale)
        {
            return FromKilometres(a.Kilometres * scale);
        }

        public static Distance operator /(Distance a, double quotient)
        {
            return FromKilometres(a.Kilometres / quotient);
        }

        public static Distance operator /(Distance a, Distance b)
        {
            return FromKilometres(a.Kilometres / b.Kilometres);
        }

        public static bool operator >(Distance left, Distance right)
        {
            return left.Kilometres > right.Kilometres;
        }

        public static bool operator <(Distance left, Distance right)
        {
            return left.Kilometres < right.Kilometres;
        }

        public static bool operator >=(Distance left, Distance right)
        {
            return left.Kilometres >= right.Kilometres;
        }

        public static bool operator <=(Distance left, Distance right)
        {
            return left.Kilometres <= right.Kilometres;
        }

        public void Add(Distance distance)
        {
            this.Kilometres += distance.Kilometres;
        }
    }
}
