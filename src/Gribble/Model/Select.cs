﻿using System.Collections.Generic;
using System.Linq;

namespace Gribble.Model
{
    public class Select
    {
        public enum TopValueType
        {
            Count,
            Percent
        }

        public int Top;
        public TopValueType TopType;
        public bool HasTop { get { return Top > 0; } }

        public int Start;
        public bool HasStart { get { return Start > 0; } }

        public bool First;
        public bool FirstOrDefault;

        public bool Count;

        public bool Randomize;

        public bool HasProjection { get { return Projection != null && Projection.Any(); } }
        public IList<SelectProjection> Projection;

        public Data Source = new Data { Type = Data.DataType.Table };
        public Data Target = new Data { Type = Data.DataType.Query };

        public Operator Where;
        public bool HasWhere { get { return Where != null; } }

        public IList<Projection> Distinct;
        public bool HasDistinct { get { return Distinct != null && Distinct.Any(); } }

        public IList<OrderByProjection> OrderBy;
        public bool HasOrderBy { get { return OrderBy != null && OrderBy.Any(); } }

        public IList<SetOperation> SetOperatons;
        public bool HasSetOperations { get { return SetOperatons != null && SetOperatons.Any(); } }
        public bool HasIntersections { get { return SetOperatons != null && SetOperatons.Any(x => x.Type == SetOperation.OperationType.Intersect); } }
        public bool HasCompliments { get { return SetOperatons != null && SetOperatons.Any(x => x.Type == SetOperation.OperationType.Compliment); } }

        public bool HasConditions
        { get { return HasTop || HasStart || First || FirstOrDefault || Count || Randomize || HasProjection ||
                   HasWhere || HasDistinct || HasOrderBy || HasSetOperations; } }
        
    }
}