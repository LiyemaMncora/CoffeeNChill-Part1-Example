using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CoffeeNChillFunctions;

public class DocumentFunctions
{
    private readonly BlobContainerClient blobCntnr;

    public DocumentFunctions(BlobServiceClient blobSvcClient)
    {
        blobCntnr = blobSvcClient.GetBlobContainerClient("staff-docs");
        blobCntnr.CreateIfNotExists();
    }

    [Function("UploadStaffDocument")]
    public async Task<IActionResult> UploadStaffDocument(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest req)
    {
        if (req.Form.Files.Count == 0)
        {
            return new BadRequestObjectResult("error: no file was attached. Attach a file under the 'file' form-data key.");
        }

        IFormFile objFormFile = req.Form.Files[0];
        string strFileName = objFormFile.FileName;

        var blobFile = blobCntnr.GetBlobClient(strFileName);

        using (Stream fileStream = objFormFile.OpenReadStream())
        {
            await blobFile.UploadAsync(fileStream, overwrite: true);
        }

        return new OkObjectResult($"file '{strFileName}' uploaded successfully to staff-docs.");
    }

    [Function("ListStaffDocuments")]
    public async Task<IActionResult> ListStaffDocuments(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req)
    {
        var lstDocuments = new List<object>();

        await foreach (var objBlob in blobCntnr.GetBlobsAsync())
        {
            lstDocuments.Add(new
            {
                FileName = objBlob.Name,
                SizeInBytes = objBlob.Properties.ContentLength,
                LastModified = objBlob.Properties.LastModified
            });
        }

        return new OkObjectResult(lstDocuments);
    }

    [Function("DownloadStaffDocument")]
    public async Task<IActionResult> DownloadStaffDocument(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest req,
        string fileName)
    {
        string strFileName = fileName;
        var blobFile = blobCntnr.GetBlobClient(strFileName);

        if (!await blobFile.ExistsAsync())
        {
            return new NotFoundObjectResult($"error: file '{strFileName}' was not found in staff-docs.");
        }

        Stream blobStream = await blobFile.OpenReadAsync();
        return new FileStreamResult(blobStream, "application/octet-stream")
        {
            FileDownloadName = strFileName
        };
    }
}