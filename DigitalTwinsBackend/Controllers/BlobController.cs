using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;

using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Hubs;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;

namespace DigitalTwinsBackend.Controllers
{
    public class BlobController : BaseController
    {
        public BlobController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }

        public ActionResult Create(ParentType blobType, Guid parentId)
        {
            BlobContentViewModel model = new BlobContentViewModel(blobType, _cache);
            model.SelectedBlobContentItem.ParentId = parentId;
            SendViewData();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BlobContentViewModel model)
        {
            try
            {
                model.SelectedBlobContentItem.Sharing = "None";

                await DigitalTwinsHelper.CreateOrUpdateBlob(
                    model.SelectedBlobContentItem.ParentType, 
                    model.SelectedBlobContentItem, 
                    model.File, 
                    _cache, 
                    Loggers.SilentLogger);

                return RedirectToAction(
                    "Details", 
                    model.SelectedBlobContentItem.ParentType.ToString(), 
                    new { id = model.SelectedBlobContentItem.ParentId });
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                SendViewData();
                return View(model);
            }
        }

        public ActionResult Delete(ParentType blobType, Guid blobId)
        {
            BlobContentViewModel model = new BlobContentViewModel(blobType, _cache, blobId);
            SendViewData();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(BlobContentViewModel model)
        {
            try
            {
                await DigitalTwinsHelper.DeleteBlobAsync(model.SelectedBlobContentItem, _cache, Loggers.SilentLogger);
                return RedirectToAction(
                    "Details", 
                    model.SelectedBlobContentItem.ParentType.ToString(), 
                    new { id = model.SelectedBlobContentItem.ParentId });
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                SendViewData();
                return View(model);
            }
        }

        public ActionResult Edit(ParentType blobType, Guid blobId)
        {
            BlobContentViewModel model = new BlobContentViewModel(blobType, _cache, blobId);
            SendViewData();
            return View(model);
        }
    }
}