﻿using EcommerseApplication.DTO;
using EcommerseApplication.Models;
using EcommerseApplication.Repository;
using EcommerseApplication.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerseApplication.Controllers.AdminScope
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IshipperRequest shipperRequest;
        private readonly Ishipper shiperRepository;
        private readonly IRequest requestRepository;
        private readonly Ipartener ipartenerRepository;
      
        public AdminController(Ipartener _ipartener,IRequest _request,Ishipper ishiper,RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IshipperRequest shipperRequest)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            this.shipperRequest = shipperRequest;
            shiperRepository = ishiper;
            requestRepository = _request;
            ipartenerRepository = _ipartener;
        }
        [HttpPost]
        [Route("AddRoleToUser")]
        public async Task<IActionResult> AddUSerToSpecificRole([FromBody] AssignRolesByEmail model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Email Does not Exist" });
                }
                if (!await _roleManager.RoleExistsAsync(model.RoleName))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role Does not Exist" });
                }
                if (await _userManager.IsInRoleAsync(user, model.RoleName))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User already assigned to {model.RoleName} Role" });

                }
                await _userManager.AddToRoleAsync(user, model.RoleName);

            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
               .SelectMany(v => v.Errors)
               .Select(e => e.ErrorMessage));
                return StatusCode(StatusCodes.Status500InternalServerError, message);
                //{
                //    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                //   "title": "One or more validation errors occurred.",
                //   "status": 400,
                //   "traceId": "00-3bd746565e6013fab2c615e972c050d0-3670e1a7eeef39c4-00",
                //    "errors": {
                //        "Email": [
                //          "Email is required"
                //       ],
                //      "RoleName": [
                //     "Role Name is required"
                //         ]
                //            }
                //}
            }

            return Ok(new Response { Status = "Ok", Message = "Created Successfuly" });
        }
        [HttpPost]
        [Route("DeleteSubAdmin")]
        public async Task<IActionResult> DeleteSubAdmin([FromBody] string Email)
        {
            var user=await _userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Email is incorrect" });
            }
            if (!await _userManager.IsInRoleAsync(user, "SubAdmin"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not assigned to A subAdmin Role" });

            }
            await _userManager.RemoveFromRoleAsync(user, "SubAdmin");
            return Ok(new Response { Status="Done",Message="Removed Successfuly"});
        }

        

        [HttpPost]
        [Route("CreateShiper")]
        public async Task<IActionResult> CreateShipper([FromBody] shiperDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Data is incorect" });
            }
            shiperRepository.insert(model);
            return Ok(new Response { Status = "Ok", Message = "Created Successfuly,Assigned Successfuly" });
        }

        /***********/
        [HttpPost]
        [Route("CreateShiperFromRequests")]
        public IActionResult CreateShipperfromRequests(int requestId)
        {
            ShipperRequest shipperR = shipperRequest.Get(requestId);
            if (shipperR != null)
            {
                Shipper shipper = new Shipper();
                shipper.Name = shipperR.Name;
                shipper.officePhone = shipperR.officePhone;
                shipper.arabicName = shipperR.arabicName;
                shipper.IdentityId = shipperR.AccountID;
                shiperRepository.insert(shipper);

                shipperRequest.remove(requestId);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Data is incorect" });
            }
            return Ok(new Response { Status = "Ok", Message = "Created Successfuly,Assigned Successfuly" });
        }


        /***********/
        [HttpPost]
        [Route("CreatePartner")]//take request id
        public IActionResult CreatePartner([FromBody]int id)
        {
           Requests request= requestRepository.GetPartnerById(id);
           User userpartner  = ipartenerRepository.getByIDentity(request.IdentityId);
            if(request==null)
            {
                return Ok(new Response { Status = "Error", Message = "Data is incorect" });
            }
            Partener partener = new Partener();
            partener.Name = request.Name;
            partener.Type = request.RequestType;
            partener.numberOfBranches = request.numberOfBranches;
            partener.IdentityId = request.IdentityId;
            partener.userID = userpartner.Id;
            ipartenerRepository.insert(partener);
            return Ok(new Response { Status = "oK", Message = "Saved" });
        }

    }
}
