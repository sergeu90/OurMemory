﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using OurMemory.Common;
using OurMemory.Common.Attributes;
using OurMemory.Common.Extention;
using OurMemory.Domain;
using OurMemory.Domain.DtoModel;
using OurMemory.Domain.Entities;
using OurMemory.Models;
using OurMemory.Service.Extenshions;
using OurMemory.Service.Interfaces;
using OurMemory.Service.Model;
using OurMemory.Service.Parsers;
using OurMemory.Service.Services;
using Image = System.Drawing.Image;
using ImageReference = OurMemory.Domain.DtoModel.ImageReference;

namespace OurMemory.Controllers
{
    [Roles(UserRoles.User, UserRoles.Administrator)]
    public class FilesController : ApiController
    {
        private readonly IImageService _imageService;
        private readonly VeteranService _veteranService;
        private readonly IUserService _userService;

        readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Work with files 
        /// </summary>
        /// <param name="imageService"></param>
        /// <param name="veteranService"></param>
        /// <param name="userManager"></param>
        public FilesController(IImageService imageService, VeteranService veteranService, IUserService userService)
        {
            _imageService = imageService;
            _veteranService = veteranService;
            _userService = userService;
        }
        /// <summary>
        /// Get report excell file 
        /// </summary>
        /// <param name="searchVeteranModel"></param>
        /// <returns></returns>
        [Route("api/files")]
        [AllowAnonymous]
        public IHttpActionResult GetReportExcellFiles([FromUri]SearchVeteranModel searchVeteranModel)
        {
            List<Veteran> searchVeterans;

            if (searchVeteranModel == null)
            {
                searchVeterans = _veteranService.GetAll().ToList();
            }
            else
            {
                searchVeterans = _veteranService.SearchVeterans(searchVeteranModel).Pagination((searchVeteranModel.Page - 1) * searchVeteranModel.Size, searchVeteranModel.Size).ToList(); ;
            }

            var veteranMappings = Mapper.Map<IEnumerable<Veteran>, IEnumerable<VeteranMapping>>(searchVeterans);

            var fileName = ExcellParser.GenerateReport(veteranMappings.ToList());

            var generateAbsolutePath = GenerateAbsolutePath(fileName);

            return Ok(new
            {
                PathToFile = generateAbsolutePath
            });
        }

        /// <summary>
        /// Upload a image files
        /// </summary>
        /// <returns></returns>
        [Route("api/files")]
        public async Task<IHttpActionResult> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            string root = HttpContext.Current.Server.MapPath("~/Content/Files/");
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var imageUrl = GenerateAbsolutePath(HttpContext.Current.Request.ApplicationPath + ConfigurationSettingsModule.GetItem("PathImages"));

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            Dictionary<string, string> errors = new Dictionary<string, string>();

            List<ImageReference> imageFilesVeterans = _imageService.SaveImages(provider, root, ref errors);

            var imageVeterans = imageFilesVeterans.Select(file => new ImageReference
            {
                ImageOriginal = imageUrl + @"/" + file.ImageOriginal,
                ThumbnailImage = imageUrl + @"/" + file.ThumbnailImage
            }).ToList();

            return Ok(new
            {
                Images = imageVeterans,
                Errors = errors
            });

        }

        /// <summary>
        /// Upload excell file on server
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Route("api/files/uploadExcell")]
        public async Task<IHttpActionResult> PostUploadExcellFiles()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            var provider = new MultipartMemoryStreamProvider();
            string path = null;

            Tuple<byte[], HttpContent> fileArrayAndFileHttpContent = await GetFileArrayAndFileHttpContentFromProvider(provider);

            var filename = GetFilename(fileArrayAndFileHttpContent.Item2);
            filename += Guid.NewGuid() + ".xlsx";

            path = Path.Combine(HttpContext.Current.Server.MapPath("~" + ConfigurationSettingsModule.GetItem("Temp")), filename);

            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~" + ConfigurationSettingsModule.GetItem("Temp"))))
            {
                Directory.CreateDirectory(
                  HttpContext.Current.Server.MapPath("~" + ConfigurationSettingsModule.GetItem("Temp")));
            }

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(fileArrayAndFileHttpContent.Item1, 0, fileArrayAndFileHttpContent.Item1.Length);
            }

            try
            {
                ExcellParser excellParser = new ExcellParser(path);
                var veteranMappings = excellParser.GetVeterans();

                foreach (var veteranMapping in veteranMappings)
                {
                    var veteranBindingModel = Mapper.Map<VeteranMapping, VeteranBindingModel>(veteranMapping);

                    var listParsedUrls = UrlParser.Parse(veteranMapping.UrlImages);
                    veteranBindingModel.Images = UrlParser.DownloadFromUrls(listParsedUrls);

                    var veteran = Mapper.Map<VeteranBindingModel, Veteran>(veteranBindingModel);
                    veteran.User = _userService.GetById(User.Identity.GetUserId());

                    var googleMapsService = new GoogleMapsService(string.Empty);
                    var latLng = googleMapsService.GetLatLng(veteran.BirthPlace);
                    veteran.Latitude = latLng.Latitude;
                    veteran.Longitude = latLng.Longitude;

                    _veteranService.Add(veteran);
                }
            }
            catch (Exception exception)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Problems with parsing the file"),
                    ReasonPhrase = "File Not Parsed"
                };

                _logger.Error(exception);

                throw new HttpResponseException(resp);
            }
            finally
            {
                if (File.Exists(path))
                {

                    File.Delete(path);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Crop a image by coordinates
        /// x,y,widh,heigh
        /// </summary>
        /// <returns></returns>
        [Route("api/files/cropImage")]
        public async Task<IHttpActionResult> CropImage([FromUri]SelectionImageBindingModel selectionImage)
        {
            var source = new Bitmap(HostingEnvironment.MapPath(selectionImage.UrlImage.ToRelativePath()));

            Rectangle section = new Rectangle(new Point(selectionImage.X, selectionImage.Y), new Size(selectionImage.Width, selectionImage.Height));
            var cropImage = _imageService.CropImage(source, section);

            var imageReference = _imageService.SaveImage(cropImage);

            return Ok(imageReference);
        }


        [System.Web.Http.NonAction]
        public string GenerateAbsolutePath(string virtualPath)
        {
            return HttpContext.Current.Request.Url.Scheme +
                           "://"
                           + HttpContext.Current.Request.Url.Authority + virtualPath;
        }



        private static string GetFilename(HttpContent file)
        {
            var filename = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

            return filename;
        }

        private async Task<Tuple<byte[], HttpContent>> GetFileArrayAndFileHttpContentFromProvider(MultipartMemoryStreamProvider provider)
        {
            await Request.Content.ReadAsMultipartAsync(provider);

            var file = provider.Contents[0];
            byte[] fileArray = file.ReadAsByteArrayAsync().Result;

            return new Tuple<byte[], HttpContent>(fileArray, file);
        }
    }
}
