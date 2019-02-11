using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Fact.Extensions.Collection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Fact.Extensions.Experimental
{
    public abstract class AccessorFileProvider<TValue> : 
        IFileProvider
    {
        readonly IAccessor<string, TValue> files;

        protected class FileInfoBase
        {
            readonly string name;
            protected readonly TValue value;
            readonly AccessorFileProvider<TValue> parent;

            protected FileInfoBase(AccessorFileProvider<TValue> parent, TValue value, string name)
            {
                this.name = name;
                this.value = value;
                this.parent = parent;
            }
            
            public string Name => name;

            public bool IsDirectory => false;
        }

        protected AccessorFileProvider(IAccessor<string, TValue> files)
        {
            this.files = files;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        protected abstract IFileInfo CreateFileInfo(string name, TValue value);

        public IFileInfo GetFileInfo(string subpath)
        {
            TValue value = files[subpath];
            return CreateFileInfo(subpath, value);
        }

        internal class ChangeToken : IChangeToken
        {
            readonly string filter;
            readonly AccessorFileProvider<TValue> parent;
            bool hasChanged = false;
            internal class Item
            {
                internal Action<object> callback;
                internal object state;
            }
            List<Item> callbacks = new List<Item>();

            internal ChangeToken(AccessorFileProvider<TValue> parent, string filter)
            {
                this.parent = parent;
                this.filter = filter;
            }

            public bool HasChanged
            {
                get => hasChanged;
                internal set
                {
                    hasChanged = true;
                    foreach (var i in callbacks) i.callback(i.state);
                }
            }

            public bool ActiveChangeCallbacks => callbacks.Count > 0;

            public IDisposable RegisterChangeCallback(Action<object> callback, object state)
            {
                callbacks.Add(new Item { callback=callback, state=state });
                return null;
            }
        }


        public IChangeToken Watch(string filter)
        {
            if (files is INotifyPropertyChanged notifier)
            {
                var token = new ChangeToken(this, filter);
                // TODO: Optimize this to be one wholistic notifier
                notifier.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == filter) token.HasChanged = true;
                };
                return token;
            }

            throw new NotImplementedException();
        }
    }


    public class ByteArrayFileProvider : AccessorFileProvider<byte[]>
    {
        protected class FileInfo : FileInfoBase, IFileInfo
        {
            internal FileInfo(AccessorFileProvider<byte[]> parent, byte[] value, string name) : 
                base(parent, value, name)
            {
            }

            public bool Exists => true;

            public long Length => value.Length;

            public string PhysicalPath => throw new NotImplementedException();

            public DateTimeOffset LastModified => throw new NotImplementedException();

            public Stream CreateReadStream() => new MemoryStream(value);
        }

        public ByteArrayFileProvider(IAccessor<string, byte[]> files) : base(files)
        {
        }

        protected override IFileInfo CreateFileInfo(string name, byte[] value)
        {
            return new FileInfo(this, value, name);
        }
    }
}
