﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Microsoft.AspNet.Identity;
using OurMemory.Common.Attributes;
using OurMemory.Domain;
using OurMemory.Domain.DtoModel;
using OurMemory.Domain.DtoModel.ViewModel;
using OurMemory.Domain.Entities;
using OurMemory.Service.Extenshions;
using OurMemory.Service.Interfaces;
using OurMemory.Service.Model;

namespace OurMemory.Controllers
{
    /// <summary>
    /// Work with veterans entity
    /// </summary>
    [Roles(UserRoles.User, UserRoles.Administrator)]
    public class VeteranController : ApiController
    {
        private readonly IVeteranService _veteranService;
        private readonly IUserService _userService;
        private readonly IImageVeteranService _imageVeteranService;

        public VeteranController(IVeteranService veteranService, IUserService userService, IImageVeteranService imageVeteranService)
        {
            _veteranService = veteranService;
            _userService = userService;
            _imageVeteranService = imageVeteranService;
        }

        /// <summary>
        /// Get a veteran by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/veteran/{id}")]
        [ResponseType(typeof(VeteranViewModel))]
        [AllowAnonymous]
        public IHttpActionResult Get(int id)
        {
            var veteran = _veteranService.GetById(id);

            if (veteran == null)
            {
                return NotFound();
            }

            veteran.Views++;

            _veteranService.SaveArticle();

            var veteranViewModel = Mapper.Map<Veteran, VeteranViewModel>(veteran);

            return Ok(veteranViewModel);
        }
        /// <summary>
        /// Get veterans by conditional or get all veterans
        /// </summary>
        /// <param name="searchVeteranModel"></param>
        /// <returns></returns>
        [Route("api/veteran")]
        [ResponseType(typeof(VeteranViewModel))]
        [AllowAnonymous]
        public IHttpActionResult Get([FromUri]SearchVeteranModel searchVeteranModel)
        {
            IEnumerable<Veteran> veterans = null;
            int allCount = 0;

            if (searchVeteranModel == null)
            {
                veterans = _veteranService.GetAll();
                allCount = _veteranService.GetAll().Count();
            }
            else
            {
                allCount = _veteranService.SearchVeterans(searchVeteranModel).Count();
                veterans = _veteranService.SearchVeterans(searchVeteranModel).Pagination((searchVeteranModel.Page - 1) * searchVeteranModel.Size, searchVeteranModel.Size).ToList();
            }

            var veteranBindingModels = Mapper.Map<IEnumerable<Veteran>, IEnumerable<VeteranViewModel>>(veterans);

            return Ok(new
            {
                Items = veteranBindingModels,
                TotalCount = allCount
            });
        }

        /// <summary>
        /// Add a veteran
        /// </summary>
        /// <param name="veteranBindingModel"></param>
        /// <returns></returns>
        [Route("api/veteran")]
        [ResponseType(typeof(VeteranViewModel))]
        public IHttpActionResult Post(VeteranBindingModel veteranBindingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Veteran veteran = Mapper.Map<VeteranBindingModel, Veteran>(veteranBindingModel);

            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                return BadRequest();
            }

            var user = _userService.GetById(userId);

            veteran.User = user;

            _veteranService.Add(veteran);

            var veteranViewModel = Mapper.Map<Veteran, VeteranViewModel>(veteran);

            return Ok(veteranViewModel);
        }

        /// <summary>
        /// Update a veteran
        /// </summary>
        /// <param name="veteranBindingModel"></param>
        /// <returns></returns>
        [Route("api/veteran")]
        [ResponseType(typeof(VeteranViewModel))]
        public IHttpActionResult Put([FromBody]VeteranBindingModel veteranBindingModel)
        {
            var veteran = _veteranService.GetById(veteranBindingModel.Id);

            var userId = User.Identity.GetUserId();

            if (ModelState.IsValid && veteranBindingModel.Id == veteran.Id && userId == veteran.User.Id || User.IsInRole(UserRoles.Administrator))
            {
                _imageVeteranService.DeleteImages(veteran.Images);

                Mapper.Map(veteranBindingModel, veteran);
               
                _veteranService.UpdateVeteran(veteran);

                var veteranModified = Mapper.Map<Veteran, VeteranViewModel>(veteran);

                return Ok(veteranModified);
            }

            return StatusCode(HttpStatusCode.NotModified);
        }

        /// <summary>
        /// Delete a veteran by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/veteran/{id}")]
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var veteran = _veteranService.GetById(id);

            var userId = User.Identity.GetUserId();

            if (veteran != null && veteran.Id == id && veteran.User.Id == userId ||
                User.IsInRole(UserRoles.Administrator))
            {
                veteran.IsDeleted = true;
                _veteranService.SaveArticle();
                return Ok();
            }

            return BadRequest();
        }
    }
}
