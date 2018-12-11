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
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Threading;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.ViewModels
{
    public class OntologyViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;

        public IEnumerable<Ontology> OntologyList { get; set; }

        public OntologyViewModel() { }

        public OntologyViewModel(IMemoryCache memoryCache, SystemTypes? systemTypes)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

            LoadAsync(systemTypes).Wait();
        }
        private async Task LoadAsync(SystemTypes? systemTypes)
        {
            var _ontologies = await DigitalTwinsHelper.GetOntologiesWithTypes(_cache, Loggers.SilentLogger);

            if (systemTypes != null)
            {
                List<Ontology> list = new List<Ontology>();
                Ontology ontology;

                foreach (Ontology item in _ontologies)
                {
                    ontology = new Ontology() { Id = item.Id, Name = item.Name, Loaded = item.Loaded };
                    ontology.types = item.types.FindAll(t => t.Category.Equals(systemTypes.ToString()));
                    list.Add(ontology);
                }
                _ontologies = list;
            }

            OntologyList = _ontologies;
        }
    }
}
