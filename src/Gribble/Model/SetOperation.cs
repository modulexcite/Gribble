﻿namespace Gribble.Model
{
    public class SetOperation
    {
        public enum OperationType
        {
            Intersect,
            Compliment
        }

        public OperationType Type;
        public Select Select;
    }
}
