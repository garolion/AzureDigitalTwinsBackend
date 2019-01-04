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

        public OntologyViewModel(IMemoryCache memoryCache, Models.Types? systemTypes)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

            try
            {
                LoadAsync(systemTypes).Wait();
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                FeedbackHelper.Channel.SendMessageAsync($"Please check your settings.", MessageType.Info).Wait();
            }
        }
        private async Task LoadAsync(Models.Types? filterType)
        {
            List<Ontology> ontologies = await DigitalTwinsHelper.GetOntologiesWithTypes(_cache, Loggers.SilentLogger);
            List<Ontology> filteredOntologies = new List<Ontology>();
            Ontology ontology;

            foreach (Ontology item in ontologies)
            {
                ontology = new Ontology() { Id = item.Id, Name = item.Name, Loaded = item.Loaded };
                if (filterType != null)
                {
                    ontology.types = item.types.FindAll(t => t.Category.Equals(filterType.ToString()));
                }
                else
                {
                    ontology.types = item.types;
                }
                filteredOntologies.Add(ontology);
            }
            OntologyList = filteredOntologies;
        }
    }
}
