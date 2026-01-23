namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Controllers
{
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;

    [ApiController]
    [Authorize]
    public abstract class BaseApiCommandController : ControllerBase
    {
        private ICommandBus commandBus = null!;

        protected ICommandBus CommandBus => this.commandBus ??= this.HttpContext.RequestServices.GetRequiredService<ICommandBus>();
    }
}