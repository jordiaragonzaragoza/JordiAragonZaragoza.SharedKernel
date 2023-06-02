﻿namespace JordiAragon.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using Ardalis.Result;
    using MediatR;

    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>
    {
    }
}