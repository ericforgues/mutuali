﻿using System.IO;
using System.Threading.Tasks;
using YellowDuck.Api.Constants;
using YellowDuck.Api.Services.Files;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Resolvers;

namespace YellowDuck.Api.Plugins.ImageSharp
{
    public class AppImageCache : IImageCache
    {
        private readonly IFileManager fileManager;

        public AppImageCache(IFileManager fileManager)
        {
            this.fileManager = fileManager;
        }

        public async Task<IImageCacheResolver> GetAsync(string key)
        {
            var fileExists = await fileManager.Exists(FileContainers.ImageCache, key);
            
            return fileExists
                ? new AppImageResolver(fileManager, FileContainers.ImageCache, key)
                : (IImageCacheResolver) null;
        }

        public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
        {
            return fileManager.UploadFile(FileContainers.ImageCache, new FileInfos
            {
                FileName = key,
                UseFileNameAsFileId = true,
                ContentType = metadata.ContentType,
                Content = stream,
                Metadata = metadata.ToDictionary()
            });
        }
    }
}