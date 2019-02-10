using System;
using System.IO;
using Fact.Extensions.Collection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Fact.Extensions.Experimental
{
    public abstract class AccessorFileProvider<TValue> : 
        IFileProvider
    {
        readonly INamedAccessor<TValue> files;

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

        protected AccessorFileProvider(INamedAccessor<TValue> files)
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

        public IChangeToken Watch(string filter)
        {
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

        public ByteArrayFileProvider(INamedAccessor<byte[]> files) : base(files)
        {
        }

        protected override IFileInfo CreateFileInfo(string name, byte[] value)
        {
            return new FileInfo(this, value, name);
        }
    }
}
