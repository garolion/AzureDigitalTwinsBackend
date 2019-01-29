using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinsBackend.Controllers
{
    public class PropertyKeyController : BaseController
    {
        List<UISpace> spaces;

        public PropertyKeyController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }

        [HttpGet]
        public IActionResult Create(Guid spaceId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            PropertyKeyViewModel model = new PropertyKeyViewModel(_cache);
            if (spaceId != Guid.Empty)
            {
                model.SelectedPropertyKey = new PropertyKey() { SpaceId = spaceId };
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PropertyKeyViewModel model)
        {
            try
            {
                var id = await DigitalTwinsHelper.CreatePropertyKeyAsync(model.SelectedPropertyKey, _cache, Loggers.SilentLogger);
                await FeedbackHelper.Channel.SendMessageAsync($"PropertyKey with id '{id}' successfully created.", MessageType.Info);

                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Add(Guid spaceId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            if (spaceId != Guid.Empty)
            {
                CacheHelper.SetObjectId(_cache, spaceId);
                Space space = await DigitalTwinsHelper.GetSpaceAsync((Guid)spaceId, _cache, Loggers.SilentLogger);
                var pksForSpace = (await DigitalTwinsHelper.GetPropertyKeysForSpace((Guid)spaceId, _cache, Loggers.SilentLogger)).ToList<PropertyKey>();

                // we filter to remove all the properties already known in the Space
                IEnumerable<PropertyKey> pks =
                    from pk in pksForSpace
                    where !space.Properties.Any(prop => prop.Name.Equals(pk.Name))
                    select pk; 

                return View(pks);
            }
            else
            {
                //TODO replace with default view (List)
                return RedirectToAction(nameof(PropertyKeyController.Create));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(List<PropertyKey> model)
        {
            try
            {
                Space space = await DigitalTwinsHelper.GetSpaceAsync(CacheHelper.GetObjectId(_cache), _cache, Loggers.SilentLogger);

                if (space.Properties == null)
                {
                    space.Properties = new System.Collections.ObjectModel.ObservableCollection<Property>();
                }

                foreach (PropertyKey pk in model)
                {
                    if (pk.Add)
                    {
                        switch (pk.PrimitiveDataType)
                        {
                            case PrimitiveDataType.Bool:
                                {
                                    space.Properties.Add(new Property()
                                    {
                                        Name = pk.Name,
                                        Value = "false"
                                    });
                                    break;
                                }
                            case PrimitiveDataType.Int:
                            case PrimitiveDataType.Long:
                            case PrimitiveDataType.Uint:
                                {
                                    space.Properties.Add(new Property()
                                    {
                                        Name = pk.Name,
                                        Value = "0"
                                    });
                                    break;
                                }
                            default:
                                {
                                    space.Properties.Add(new Property()
                                    {
                                        Name = pk.Name,
                                        Value = ""
                                    });
                                    break;
                                }
                        }
                    }
                }

                await DigitalTwinsHelper.UpdateSpacePropertiesAsync(space, _cache, Loggers.SilentLogger);
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View();
            }
        }

        public async Task<IActionResult> Remove(Guid spaceId, string name)
        {
            if (spaceId != Guid.Empty && name != null && name != string.Empty)
            {
                Space space = await DigitalTwinsHelper.GetSpaceAsync((Guid)spaceId, _cache, Loggers.SilentLogger);
                var prop = space.Properties.FirstOrDefault(p => p.Name.Equals(name));
                if (prop != null)
                {
                    space.Properties.Remove(prop);
                    await DigitalTwinsHelper.UpdateSpaceAsync(space, _cache, Loggers.SilentLogger);
                }

                return Redirect(Request.Headers["Referer"].ToString());
            }
            else
            {
                //TODO replace with default view (List)
                return RedirectToAction(nameof(PropertyKeyController.Create));
            }
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            spaces = new List<UISpace>
            {
                new UISpace() { Space = new Space() { Name = "Root", Id = Guid.Empty }, MarginLeft = "0" }
            };

            var propertyKeys = await DigitalTwinsHelper.GetPropertyKeys(_cache, Loggers.SilentLogger);
            foreach (var propertyKey in propertyKeys)
            {
                await MergeTree(propertyKey, propertyKey.SpacesHierarchy, 0);
            }

            return View(spaces.Skip(1));
        }

        private async Task MergeTree(PropertyKey propertyKey, IEnumerable<Guid> spacePath, int level)
        {
            level++;
            Guid spaceId = spacePath.First();
            Space space = await DigitalTwinsHelper.GetSpaceAsync(spaceId, _cache, Loggers.SilentLogger);

            if (spacePath.Count() == 1)
            {
                if (!space.PropertyKeys.Exists(p => p.Id == propertyKey.Id))
                {
                    space.PropertyKeys.Add(propertyKey);
                }
            }

            if (!spaces.Exists(s => s.Space.Id == space.Id))
            {
                int index = spaces.FindIndex(s => s.Space.Id == space.ParentSpaceId);
                spaces.Insert(index + 1, new UISpace() { Space = space, MarginLeft = $"{25 * level-1}px" });
            }
            else
            {
                spaces.First(s => s.Space.Id == space.Id).Space = space;
            }

            if (spacePath.Count() > 1)
            {
                await MergeTree(propertyKey, spacePath.Skip(1), level);
            }
        }
    }
}