// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;

namespace DataDrain.ORM.Data.Common.Language
{
    public abstract class QueryType
    {
        public abstract bool NotNull { get; }
        public abstract int Length { get; }
        public abstract short Precision { get; }
        public abstract short Scale { get; }
    }

    public abstract class QueryTypeSystem 
    {
        public abstract QueryType Parse(string typeDeclaration);
        public abstract QueryType GetColumnType(Type type);
        public abstract string GetVariableDeclaration(QueryType type, bool suppressSize);
    }
}