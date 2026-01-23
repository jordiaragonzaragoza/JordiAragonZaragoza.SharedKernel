namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Controllers
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;

    [ApiController]
    [Authorize]
    public abstract class BaseApiQueryController : ControllerBase
    {
        private IQueryBus queryBus = null!;

        protected IQueryBus QueryBus => this.queryBus ??= this.HttpContext.RequestServices.GetRequiredService<IQueryBus>();
    }
}