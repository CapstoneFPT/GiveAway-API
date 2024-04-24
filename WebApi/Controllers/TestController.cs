using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/testing")]
public class TestController  : ControllerBase
{
   [Authorize(Roles = "Admin")]
   [HttpGet("admin")]
   public string TestAdmin()
   {
      return "Admin Okay";
   }

   [Authorize(Roles = "Staff")]
   [HttpGet("staff")]
   public string TestStaff()
   {
      return "Staff Okay";
   }

   [Authorize(Roles = "User")]
   [HttpGet("user")]
   public string TestUser()
   {
      return "User Okay";
   }
}