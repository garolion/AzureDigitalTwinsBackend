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
        public PropertyKeyController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;

            
        }

        public IActionResult Create(Guid spaceId)
        {
            PropertyKeyViewModel model = new PropertyKeyViewModel(_cache);

            if (spaceId != Guid.Empty)
            {
                model.SelectedPropertyKey = new PropertyKey() { SpaceId = spaceId };
                CacheHelper.SetContext(_cache, Context.Space);
            }
            else
            {
                CacheHelper.SetContext(_cache, Context.None);
            }

            //PropertyKeyViewModel model = new PropertyKeyViewModel(_cache, id: "3");

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

                if (CacheHelper.IsInSpaceEditMode(_cache))
                {
                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Space", new { id = model.SelectedPropertyKey.SpaceId });
                }
                else
                {
                    //TODO replace with default view (List) ?
                    return RedirectToAction(nameof(PropertyKeyController.Create));
                }

            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View();
            }
        }

        public async Task<IActionResult> Add(Guid spaceId)
        {
            if (spaceId != Guid.Empty)
            {
                Space space = await DigitalTwinsHelper.GetSpaceAsync((Guid)spaceId, _cache, Loggers.SilentLogger);
                var pksForSpace = (await DigitalTwinsHelper.GetPropertyKeysForSpace((Guid)spaceId, _cache, Loggers.SilentLogger)).ToList<PropertyKey>();

                // we filter to remove all the properties already known in the Space
                IEnumerable<PropertyKey> pks =
                    from pk in pksForSpace
                    where !space.Properties.Any(prop => prop.Name.Equals(pk.Name))
                    select pk; 

                CacheHelper.SetContext(_cache, Context.Space);
                CacheHelper.SetSpaceId(_cache, spaceId);

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
                // Add properties

                if (CacheHelper.IsInSpaceEditMode(_cache))
                {
                    Space space = await DigitalTwinsHelper.GetSpaceAsync(CacheHelper.GetSpaceId(_cache), _cache, Loggers.SilentLogger);

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
                                            //DataType = pk.PrimitiveDataType.ToString(),
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
                                            //DataType = pk.PrimitiveDataType.ToString(),
                                            Name = pk.Name,
                                            Value = "0"
                                        });
                                        break;
                                    }
                                default:
                                    {
                                        space.Properties.Add(new Property()
                                        {
                                            //DataType = pk.PrimitiveDataType.ToString(),
                                            Name = pk.Name,
                                            Value = ""
                                        });
                                        break;
                                    }
                            }
                        }
                    }
                    
                    await DigitalTwinsHelper.UpdateSpacePropertiesAsync(space, _cache, Loggers.SilentLogger);

                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Space", new { id = CacheHelper.GetSpaceId(_cache) });
                }
                else
                {
                    //TODO replace with default view (List) ?
                    return RedirectToAction(nameof(PropertyKeyController.Create));
                }

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

                return RedirectToAction("Edit", "Space", new { id = spaceId });
            }
            else
            {
                //TODO replace with default view (List)
                return RedirectToAction(nameof(PropertyKeyController.Create));
            }
        }
    }
}