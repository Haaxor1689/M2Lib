﻿namespace M2Lib.types
{
    /// <summary>
    ///     A one dimensional range defined by the bounds.
    /// </summary>
    public readonly struct CRange
    {
        public readonly float Min,
            Max;

        public CRange(float p1, float p2)
        {
            Min = p1;
            Max = p2;
        }

        public override string ToString()
        {
            return $"({Min}->{Max})";
        }
    }
}
