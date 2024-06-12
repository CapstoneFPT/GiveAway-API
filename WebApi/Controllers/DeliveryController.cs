using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Deliveries;

namespace WebApi.Controllers
{
    [Route("api/deliveries")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _delivery;

        public DeliveryController(IDeliveryService delivery)
        {
            _delivery = delivery;
        }

    }
}
