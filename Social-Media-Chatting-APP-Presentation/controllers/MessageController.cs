using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Social_Media_Chatting_APP_Presentation.Controllers;


[ApiController]
[Route("api/[controller]")]
public class MessageController(
    ISender sender
    ) : ApiBaseController
{
  
    
    
}