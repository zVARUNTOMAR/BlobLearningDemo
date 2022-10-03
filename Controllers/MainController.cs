using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using AzureBlobStorageDemo.models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.VisualBasic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;

namespace AzureBlobStorageDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly QueueClient _queue;
        private readonly IWebHostEnvironment _webHostEnviornment;

        public MainController(IConfiguration configuration, QueueClient queue, IWebHostEnvironment webHostEnviornment)
        {
            _configuration = configuration;
            _queue = queue;
            _webHostEnviornment = webHostEnviornment;
        }

        [HttpGet]
        public IActionResult GetAllBlobs()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["StorageAccountConnectionString"]);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("image");
            IEnumerable<dynamic> blobs = blobContainerClient.GetBlobs().ToList();
            List<dynamic> blobUrls = new List<dynamic>();

            foreach (dynamic blob in blobs)
            {
                BlobClient blobClient = blobContainerClient.GetBlobClient(blob.Name);
                blobUrls.Add(new { blobClient.Uri, blobClient.Name });
            }
            return Ok(blobUrls);
        }

        [HttpPost]
        public IActionResult AddMessage()
        {
            User user = new User();
            Random random = new Random();
            user.Id = random.Next(100);
            user.Name = "Varun";
            user.Age = random.Next(1, 100);

            var message = JsonSerializer.Serialize(user);
            _queue.SendMessage(message);
            return Ok();
        }

        [HttpGet("GetMessages")]
        public IActionResult GetMessages()
        {
            try
            {
                /* QueueClient queueClient = new QueueClient(_configuration["StorageAccountConnectionString"], "devqueue");*/
                dynamic data = _queue.ReceiveMessage();
                var message = JsonSerializer.Deserialize<object>(data.Value.MessageText);
                if (data.Value != null)
                {
                    _queue.DeleteMessage(data.Value.MessageId, data.Value.PopReceipt);
                }
                return Ok(message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("PeekMessage")]
        public IActionResult PeekMessages()
        {
            try
            {
                dynamic data = _queue.PeekMessages(32);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["StorageAccountConnectionString"]);
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("image");

                //string wwwRootPath = _webHostEnviornment.WebRootPath;
                //string uploads = Path.Combine(@"UploadFiles");
                //string extension = Path.GetExtension(image.FileName);
                //string fullFileName = image.FileName + DateTime.Now.ToString("yymmssfff") + extension;

                //string path = Path.Combine(uploads, fullFileName);

                /*using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    *//*image.CopyTo(stream);*//*
                    stream.Position = 0;
                    await blobContainerClient.UploadBlobAsync(image.FileName, stream);
                };
*/
                using (MemoryStream stream = new MemoryStream())
                {
                    image.CopyTo(stream);
                    stream.Position = 0;
                    await blobContainerClient.UploadBlobAsync(image.FileName, stream);
                }


                /* if (string.IsNullOrWhiteSpace(_webHostEnviornment.WebRootPath))
                 {
                     _webHostEnviornment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                 }*//*
                string wwwrootPath = Path.Combine(_webHostEnviornment.ContentRootPath, "UploadFiles");
                string filePath = Path.Combine(wwwrootPath, image.FileName);
                *//* var uploads = Path.Combine(wwwrootPath, "");
                var extension = Path.GetExtension(image.FileName)*//*;

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    blobContainerClient.UploadBlobAsync(image.FileName, stream);
                };*/

                /*PeekedMessage[] peekMessages = new PeekMessag(); */
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
