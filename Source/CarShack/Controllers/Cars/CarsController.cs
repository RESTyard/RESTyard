using System;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using CarShack.Hypermedia;
using CarShack.Util;
using FunicularSwitch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.WebApi;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.MediaTypes;

namespace CarShack.Controllers.Cars
{
    [Route("Cars/")]
    [ApiController]
    public class CarsController : Controller
    {
        private readonly HypermediaCarsRootHto carsRoot;

        public CarsController(HypermediaCarsRootHto carsRoot)
        {
            this.carsRoot = carsRoot;
        }

        [HttpGet(""), HypermediaObjectEndpoint<HypermediaCarsRootHto>]
        public ActionResult GetRootDocument()
        {
            return Ok(carsRoot);
        }

        // example route with more than one placeholder variable. Mapping of object keys to those parameters when creating links
        // is handled by using KeyAttribute on HypermediaCar instead of passing RouteKeyProducer type in HttpGetHypermediaObject attribute.
        [HttpGet("{brand}/{id:int}"), HypermediaObjectEndpoint<HypermediaCarHto>]
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

        [HttpGet("special/{brand}/{id:int}"), HypermediaObjectEndpoint<DerivedCarHto>]
        public ActionResult GetDerivedEntity(string brand, int id)
        {
            try
            {
                // short cut for example, we should actually call the Car repo and get a Car domain object
                var result = new DerivedCarHto(
                    id: id,
                    brand: brand,
                    priceDevelopment: [],
                    popularCountries: [],
                    mostPopularIn: new Country(),
                    lastInspection: new DateOnly(2025, 09, 02),
                    derivedProperty: "some text",
                    derivedOperation: new(() => false),
                    updateInspection: new(() => true),
                    item: [],
                    derivedLinkKey: Option<HypermediaCustomerHto.Key>.None);
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
        }

        [HttpGet("CarImage/{filename}"), HypermediaObjectEndpoint<CarImageHto>]
        public async Task<IActionResult> GetCarImage(string filename)
        {
            var fullPath = GetFilePath(filename);

            var content = await System.IO.File.ReadAllBytesAsync(fullPath);

            return new FileContentResult(content, MediaTypeNames.Image.Jpeg);
        }

        [HttpGet("CarInsurance/{filename}"), HypermediaObjectEndpoint<CarInsuranceHto>]
        public async Task<IActionResult> GetCarInsurance(string filename)
        {
            var fullPath = GetFilePath(filename);

            var content = await System.IO.File.ReadAllBytesAsync(fullPath);

            return new FileContentResult(content, MediaTypeNames.Application.Pdf);
        }

        [HttpPost("UploadImage"), HypermediaActionEndpoint<HypermediaCarsRootHto>(nameof(HypermediaCarsRootHto.UploadCarImage), DefaultMediaTypes.MultipartFormData)]
        public async Task<IActionResult> UploadCarImage(
            [HypermediaUploadParameterFromForm]
            HypermediaFileUploadActionParameter<UploadCarImageParameters> parameters)
        {
            var files = parameters.Files;
            if (files.Count != 1)
            {
                return BuildMalformedRequestError();
            }

            var payloadFile = files[0];
            var maxFileSize = 1024 * 1024 * 4;
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
            var path = await SaveToBinDir(originalFilename, payloadFile);

            return this.Created(Link.To(new CarImageHto(Path.GetFileName(path))));
        }

        [HttpPost("UploadInsurance"), HypermediaActionEndpoint<HypermediaCarsRootHto>(nameof(HypermediaCarsRootHto.UploadInsuranceScan), DefaultMediaTypes.MultipartFormData)]
        public async Task<IActionResult> UploadInsurance(
            [HypermediaUploadParameterFromForm] HypermediaFileUploadActionParameter parameters)
        {
            var files = parameters.Files;
            if (files.Count != 1)
            {
                return BuildMalformedRequestError();
            }
            
            // do things with the data
            var payloadFile = files[0];
            var path = await SaveToBinDir(payloadFile.FileName, payloadFile);

            return this.Created(Link.To(new CarInsuranceHto(Path.GetFileName(path))));
        }

        [HttpPatch("{brand}/{id:int}/UpdateInspection")]
        [HypermediaActionEndpoint<HypermediaCarHto>(nameof(HypermediaCarHto.UpdateInspection))]
        public async Task<IActionResult> UpdateInspection(int id, string brand, UpdateCarInspection parameter)
        {
            return this.Created(Link.ByKey(new HypermediaCarHto.Key(id, brand)));
        }

        private static async Task<string> SaveToBinDir(string originalFilename, IFormFile payloadFile)
        {
            var saveToPath = GetFilePath(GetFileName(originalFilename));

            await using (var targetStream = System.IO.File.Create(saveToPath))
            {
                await payloadFile.CopyToAsync(targetStream);
            }

            return saveToPath;
        }

        private static string GetFilePath(string fileName)
        {
            // In stub load to execution folder and replace same file. In Prod dont use user supplied filename and do not store in app folder
            var executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var targetPath = Path.Combine(executionPath, "Uploads");
            Directory.CreateDirectory(targetPath);
            var saveToPath = Path.Combine(targetPath, GetFileName(fileName));
            return saveToPath;
        }

        private static string GetFileName(string originalFilename)
        {
            return "MyUploadedFile" + Path.GetExtension(originalFilename);
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
}