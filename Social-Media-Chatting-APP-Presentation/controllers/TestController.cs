using Microsoft.AspNetCore.Mvc;

namespace Social_Media_Chatting_APP_Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ApiBaseController
    {

        [HttpGet]

        public string Get()
        {
            return "Hello World!";
        }


    }
}
