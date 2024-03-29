﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CarShack.Hypermedia;
using CarShack.Hypermedia.Cars;
using CarShack.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.MediaTypes;

namespace CarShack.Controllers.Cars
{
    [Route("Cars/")]
    public class CarsController : Controller
    {
        private readonly HypermediaCarsRootHto carsRoot;

        public CarsController(HypermediaCarsRootHto carsRoot)
        {
            this.carsRoot = carsRoot;
        }

        [HttpGetHypermediaObject("", typeof(HypermediaCarsRootHto))]
        public ActionResult GetRootDocument()
        {
            return Ok(carsRoot);
        }

        // example route with more than one placeholder variable. Mapping of object keys to those parameters when creating links
        // is handled by using KeyAttribute on HypermediaCar instead of passing RouteKeyProducer type in HttpGetHypermediaObject attribute.
        [HttpGetHypermediaObject("{brand}/{id:int}", typeof(HypermediaCarHto))]
        public ActionResult GetEntity(string brand, int id)
        {
            try
            {
                // short cut for example, we should actually call the Car repo and get a Car domain object
                var result = new HypermediaCarHto(brand, id);
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
        }

        [HttpPostHypermediaAction("UploadImage", typeof(UploadCarImageOp), AcceptedMediaType = DefaultMediaTypes.MultipartFormData)]
        public async Task<IActionResult> UploadCarImage()
        {
            // todo file size check and limit
            var request = HttpContext.Request;

            if (HttpContext.Request.Form == null)
            {
                return BuildMalformedRequestError();
            }

            // validation of Content-Type
            // 1. first, it must be a form-data request
            // 2. a boundary should be found in the Content-Type
            if (!request.HasFormContentType
                || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader)
                || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new ProblemDetails
                {
                    Title = "File upload malformed",
                    Detail = $"File upload must be form-data and with boundary",
                    Status = StatusCodes.Status415UnsupportedMediaType
                });
            }

            var files = HttpContext.Request.Form.Files;
            if (files.Count != 1)
            {
                return BuildMalformedRequestError();
            }

            var payloadFile = files[0];
            var maxFileSize = 1024*1024*4;
            if (payloadFile.Length > maxFileSize)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Title = "File too big",
                    Detail = $"File must be <={maxFileSize}",
                    Status = StatusCodes.Status400BadRequest
                });
                
            }
            var originalFilename = payloadFile.FileName;
            // Don't trust any file name, file extension, and file data from the request unless you trust them completely
            // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
            // In short, it is necessary to restrict and verify the upload
            await SaveToBinDir(originalFilename, payloadFile);

            return Ok();
        }

        private static async Task SaveToBinDir(string originalFilename, IFormFile payloadFile)
        {
            // In stub load to execution folder and replace same file. In Prod dont use user supplied filename and do not store in app folder
            var executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var targetPath = Path.Combine(executionPath, "Uploads");
            Directory.CreateDirectory(targetPath);
            var saveToPath = Path.Combine(targetPath, "MyUploadedFile" + Path.GetExtension(originalFilename));

            await using (var targetStream = System.IO.File.Create(saveToPath))
            {
                await payloadFile.CopyToAsync(targetStream);
            }
        }

        private BadRequestObjectResult BuildMalformedRequestError()
        {
            return BadRequest(new ProblemDetails()
            {
                Title = "File upload malformed",
                Detail = "Form must contain one key: 'Payload' with Value: <file to upload>",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    public class UploadCarImageOp : FileUploadHypermediaAction
    {
        public UploadCarImageOp(Func<bool> canExecute, FileUploadConfiguration fileUploadConfiguration = null) : base(canExecute, fileUploadConfiguration)
        {
        }
    }
}
