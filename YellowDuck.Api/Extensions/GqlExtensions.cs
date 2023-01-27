﻿using GraphQL.Conventions;
using System;

namespace YellowDuck.Api.Extensions
{
    public static class GqlExtensions
    {
        public static long LongIdentifierForType<T>(this Id id)
        {
            var rawId = id.IdentifierForType<T>();

            if (!long.TryParse(rawId, out var longId))
                throw new ArgumentException($"Expected valid 64-bit number but got {rawId}");

            return longId;
        }

        public static object GetInputValue(this IResolutionContext ctx)
        {
            var input = ctx.GetArgument("input");
            if (input == null) return null;

            var inputType = input.GetType();
            if (inputType.IsGenericType && inputType.GetGenericTypeDefinition() == typeof(NonNull<>))
            {
                input = inputType.GetProperty("Value").GetValue(input);
            }

            return input;
        }
    }
}
