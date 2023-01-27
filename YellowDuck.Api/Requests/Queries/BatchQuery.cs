﻿using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YellowDuck.Api.Requests.Queries
{
    public abstract class BatchQuery<TQuery, TKey, TResult> : IRequestHandler<TQuery, IDictionary<TKey, TResult>> where TQuery : IRequest<IDictionary<TKey, TResult>>
    {
        public abstract Task<IDictionary<TKey, TResult>> Handle(TQuery request, CancellationToken cancellationToken);

        public abstract class BaseQuery : IRequest<IDictionary<TKey, TResult>>, IIdListQuery<TKey>
        {
            public IEnumerable<TKey> Ids { get; set; }
        }
    }
}