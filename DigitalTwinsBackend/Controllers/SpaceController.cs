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
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Controllers
{
    public class SpaceController : BaseController
    {
        public SpaceController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }

        #region List (overview) / Details
        [HttpGet]
        public async Task<ActionResult> List()
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            var model = new SpacesViewModel(_cache);

            try
            {
                model.SpaceList = await DigitalTwinsHelper.GetRootSpacesAsync(_cache, Loggers.SilentLogger, false);
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Details(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            SpaceViewModel model = new SpaceViewModel(_cache, id);
            return View(model);
        }
        #endregion

        [HttpGet]
        public async Task<ActionResult> ViewGraph()
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            try
            {
                Graph graph = new Graph();

                var spaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);
                foreach (Space space in spaceList)
                {
                    graph.nodes.Add(new Node() { 
                        id = space.Id, 
                        label = new Label() { content = space.Label }, 
                        fill = "orange",
                        shape = NodeShape.roundedrect.ToString(),
                        nodeType = "Space" });

                    if (space.ParentSpaceId != Guid.Empty)
                    {
                        graph.edges.Add(new Edge() { id = Guid.NewGuid(), source = space.ParentSpaceId, target = space.Id });
                    }
                }

                var deviceList = await DigitalTwinsHelper.GetDevicesAsync(_cache, Loggers.SilentLogger);
                foreach (Device device in deviceList)
                {
                    graph.nodes.Add(new Node() { 
                        id = device.Id, 
                        label = new Label() { content = device.Label, fill = "white" }, 
                        fill = "green",
                        shape = NodeShape.maxroundedrect.ToString(),
                        nodeType = "Device" });

                    if (device.SpaceId != Guid.Empty)
                    {
                        graph.edges.Add(new Edge() { id = Guid.NewGuid(), source = device.SpaceId, target = device.Id });
                    }

                    if (device.Sensors != null)
                    {
                        foreach (Sensor sensor in device.Sensors)
                        {
                            graph.nodes.Add(new Node() { 
                                id = sensor.Id, 
                                label = new Label() { content = sensor.Label, fill = "white" }, 
                                fill = "blue",
                                shape = NodeShape.maxroundedrect.ToString(),
                                nodeType = "Sensor" });

                            if (sensor.DeviceId != Guid.Empty)
                            {
                                graph.edges.Add(new Edge() { id = Guid.NewGuid(), source = sensor.DeviceId, target = sensor.Id });
                            }
                        }
                    }
                }

                var udfList = await DigitalTwinsHelper.GetUserDefinedFunctions(_cache, Loggers.SilentLogger);
                foreach (UserDefinedFunction udf in udfList)
                {
                    graph.nodes.Add(new Node() { 
                        id = udf.Id, 
                        label = new Label() { content = udf.Label, fill = "white"}, 
                        fill = "brown",
                        shape = NodeShape.maxroundedrect.ToString(),
                        nodeType = "UDF" });

                    if (udf.SpaceId != Guid.Empty)
                    {
                        graph.edges.Add(new Edge() { id = Guid.NewGuid(), source = udf.SpaceId, target = udf.Id });
                    }
                }

                ViewData["sourceJSON"] = JsonConvert.SerializeObject(graph, Formatting.Indented);
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
            }

            return View();
        }


        [HttpGet]
        public ActionResult Search()
        {
            return Redirect("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Search(SpacesViewModel model)
        {
            var searchString = model.SearchString;
            var searchType = model.SearchType;

            model = new SpacesViewModel(_cache);

            int searchTypeId = searchType.Equals("All") ? -1 : model.SpaceTypeList.Single(t => t.Name.Equals(searchType)).Id;

            model.SpaceList = await DigitalTwinsHelper.SearchSpacesAsync(_cache, Loggers.SilentLogger, searchString, searchTypeId);
            return View("List", model);
        }


        #region Create
        [HttpGet]
        public ActionResult Create(Guid parentId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            SpaceViewModel model = new SpaceViewModel(_cache);

            if (parentId!=Guid.Empty)
            {
                model.SelectedSpaceItem.ParentSpaceId = parentId;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SpaceViewModel model, string createButton)
        {
            string previousPage = CacheHelper.GetPreviousPage(_cache);
            
            if (createButton.Equals("Cancel"))
            {
                return Redirect(previousPage);
            }
                       
            if (ModelState.IsValid)
            {
                try
                {
                    var spaceResult = await DigitalTwinsHelper.CreateSpaceAsync(model.SelectedSpaceItem, _cache, Loggers.SilentLogger);

                    switch (createButton)
                    {
                        case "Save & Close":
                            return Redirect(previousPage);
                        case "Save & Create another":
                            return RedirectToAction(nameof(Create));
                        case "Save & Edit":
                            if (spaceResult.Id != Guid.Empty)
                            {
                                return RedirectToAction(nameof(Edit), new { id = spaceResult.Id });
                            }
                            else
                            {
                                return Redirect(previousPage);
                            }
                        default:
                            return RedirectToAction(nameof(List));
                    }
                }
                catch (Exception ex)
                {
                    await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                    model = new SpaceViewModel(_cache);
                    return View(model);
                }
            }
            else
            {
                return View("Create");
            }
        }
        #endregion

        #region Edit / Update
        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            SpaceViewModel model = new SpaceViewModel(_cache, id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SpaceViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                await DigitalTwinsHelper.UpdateSpaceAsync(model.SelectedSpaceItem, _cache, Loggers.SilentLogger);
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                model = new SpaceViewModel(_cache, model.SelectedSpaceItem.Id);
                return View(model);
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            SpaceViewModel model = new SpaceViewModel(_cache, id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(SpaceViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                if (await DigitalTwinsHelper.DeleteSpaceAsync(model.SelectedSpaceItem, _cache, Loggers.SilentLogger))
                {
                    return Redirect(CacheHelper.GetPreviousPage(_cache));
                }
                else
                {
                    model = new SpaceViewModel(_cache, model.SelectedSpaceItem.Id);

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.InnerException.ToString(), MessageType.Info);
                return View();
            }
        }
        #endregion
    }
}