﻿using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YellowDuck.Api.Requests.Queries;

namespace YellowDuck.Api.Requests
{
    public abstract class BatchCollectionQuery<TQuery, TKey, TResult> : IRequestHandler<TQuery, ILookup<TKey, TResult>> where TQuery : IRequest<ILookup<TKey, TResult>>
    {
        public abstract Task<ILookup<TKey, TResult>> Handle(TQuery request, CancellationToken cancellationToken);

        public abstract class BaseQuery : IRequest<ILookup<TKey, TResult>>, IIdListQuery<TKey>
        {
            public IEnumerable<TKey> Ids { get; set; }
        }
    }
}
