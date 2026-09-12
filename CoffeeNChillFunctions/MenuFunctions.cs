using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using Azure.Data.Tables;
using CoffeeNChillFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CoffeeNChillFunctions;

public class MenuFunctions
{
    private readonly TableClient tblClient;

    public MenuFunctions(TableServiceClient tblSvcClient)
    {
        tblClient = tblSvcClient.GetTableClient("MenuItems");
        tblClient.CreateIfNotExists();
    }

    [Function("CreateMenuItem")]
    public async Task<IActionResult> CreateMenuItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequest req)
    {
        MenuItemEntity? objMenuItem = await req.ReadFromJsonAsync<MenuItemEntity>();

        if (objMenuItem == null || string.IsNullOrWhiteSpace(objMenuItem.PartitionKey) || string.IsNullOrWhiteSpace(objMenuItem.RowKey))
        {
            return new BadRequestObjectResult("error: Category (PartitionKey) and SKU (RowKey) are required.");
        }

        await tblClient.AddEntityAsync(objMenuItem);
        return new CreatedResult($"/api/menu/{objMenuItem.PartitionKey}/{objMenuItem.RowKey}", objMenuItem);
    }

    [Function("GetAllMenuItems")]
    public async Task<IActionResult> GetAllMenuItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequest req)
    {
        var lstMenuItems = new List<MenuItemEntity>();

        await foreach (MenuItemEntity objItem in tblClient.QueryAsync<MenuItemEntity>())
        {
            lstMenuItems.Add(objItem);
        }

        return new OkObjectResult(lstMenuItems);
    }

    [Function("GetMenuItemsByCategory")]
    public async Task<IActionResult> GetMenuItemsByCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequest req,
        string category)
    {
        string strCategory = category;
        var lstMenuItems = new List<MenuItemEntity>();

        await foreach (MenuItemEntity objItem in tblClient.QueryAsync<MenuItemEntity>(m => m.PartitionKey == strCategory))
        {
            lstMenuItems.Add(objItem);
        }

        return new OkObjectResult(lstMenuItems);
    }

    [Function("UpdateMenuItem")]
    public async Task<IActionResult> UpdateMenuItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{id}")] HttpRequest req,
        string category, string id)
    {
        string strCategory = category;
        string strId = id;
        MenuItemEntity objExistingItem;

        try
        {
            Response<MenuItemEntity> objResult = await tblClient.GetEntityAsync<MenuItemEntity>(strCategory, strId);
            objExistingItem = objResult.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new NotFoundObjectResult($"error: menu item not found for category '{strCategory}' and id '{strId}'.");
        }

        MenuItemUpdateRequest? objUpdateData = await req.ReadFromJsonAsync<MenuItemUpdateRequest>();

        if (objUpdateData == null)
        {
            return new BadRequestObjectResult("error: invalid update data provided.");
        }

        if (objUpdateData.Price.HasValue)
        {
            objExistingItem.Price = objUpdateData.Price.Value;
        }
        if (objUpdateData.IsAvailable.HasValue)
        {
            objExistingItem.IsAvailable = objUpdateData.IsAvailable.Value;
        }

        await tblClient.UpdateEntityAsync(objExistingItem, ETag.All, TableUpdateMode.Replace);
        return new OkObjectResult(objExistingItem);
    }

    [Function("DeleteMenuItem")]
    public async Task<IActionResult> DeleteMenuItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{id}")] HttpRequest req,
        string category, string id)
    {
        string strCategory = category;
        string strId = id;

        try
        {
            await tblClient.DeleteEntityAsync(strCategory, strId);
            return new NoContentResult();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new NotFoundObjectResult($"error: menu item not found for category '{strCategory}' and id '{strId}'.");
        }
    }
}

public class MenuItemUpdateRequest
{
    public double? Price { get; set; }
    public bool? IsAvailable { get; set; }
}