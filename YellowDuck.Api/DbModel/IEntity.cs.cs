﻿namespace YellowDuck.Api.DbModel
{
    public interface IHaveIdentifier { }
    public interface IHaveLongIdentifier : IHaveIdentifier { long Id { get; } }
    public interface IHaveStringIdentifier : IHaveIdentifier { string Id { get; } }
}