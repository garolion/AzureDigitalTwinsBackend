// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DigitalTwinsBackend.Models
{
    public abstract class BaseModel : IDisposable
    {
        private IntPtr handle;
        private bool disposed = false;

        public Guid Id { get; set; }
        public ObservableCollection<Property> Properties { get; set; }
        public abstract string Label { get; }
        public abstract Dictionary<string, object> ToCreate();
        public abstract Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement);
        private bool propertiesHasChanged = false;
        public bool PropertiesHasChanged
        {
            get { return propertiesHasChanged; }
        }

    public BaseModel()
        {
            Properties = new ObservableCollection<Property>();
            Properties.CollectionChanged += Properties_CollectionChanged;
        }

        private void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            propertiesHasChanged = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //Properties.Dispose();
                }

                CloseHandle(handle);
                handle = IntPtr.Zero;

                disposed = true;

            }
        }
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);

        ~BaseModel()
        {
            Dispose(false);
        }
    }
}