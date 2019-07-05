using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("Microwave.UI.UnitTests")]
namespace Microwave.UI
{
    internal class MicrowaveUiConfigureOptions : IPostConfigureOptions<StaticFileOptions>
    {
        private readonly IHostingEnvironment _environment;

        public MicrowaveUiConfigureOptions(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public void PostConfigure(string name, StaticFileOptions options)
        {
            options.ContentTypeProvider = options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();

            if (options.FileProvider == null && _environment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }

            options.FileProvider = options.FileProvider ?? _environment.WebRootFileProvider;

            var filesProvider = new ManifestEmbeddedFileProvider(GetType().Assembly, "Resources");
            options.FileProvider = new CompositeFileProvider(options.FileProvider, filesProvider);
        }
    }
}