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

        [HttpGet]
        public ActionResult Create(ParentType blobType, Guid parentId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            BlobContentViewModel model = new BlobContentViewModel(blobType, _cache);
            model.SelectedBlobContentItem.ParentId = parentId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BlobContentViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                model.SelectedBlobContentItem.Sharing = "None";

                await DigitalTwinsHelper.CreateOrUpdateBlob(
                    model.SelectedBlobContentItem.ParentType, 
                    model.SelectedBlobContentItem, 
                    model.File, 
                    _cache, 
                    Loggers.SilentLogger);

                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Edit(ParentType blobType, Guid blobId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            BlobContentViewModel model = new BlobContentViewModel(blobType, _cache, blobId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BlobContentViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                model.SelectedBlobContentItem.Sharing = "None";

                await DigitalTwinsHelper.CreateOrUpdateBlob(
                    model.SelectedBlobContentItem.ParentType,
                    model.SelectedBlobContentItem,
                    model.File,
                    _cache,
                    Loggers.SilentLogger);

                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Delete(ParentType blobType, Guid blobId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            BlobContentViewModel model = new BlobContentViewModel(blobType, _cache, blobId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(BlobContentViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                await DigitalTwinsHelper.DeleteBlobAsync(model.SelectedBlobContentItem, _cache, Loggers.SilentLogger);
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View(model);
            }
        }


    }
}