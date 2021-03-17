using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;

namespace MutagenMerger.Lib
{
    [PublicAPI]
    public sealed class Merger : IDisposable
    {
        private readonly LoadOrder<IModListing<ISkyrimModGetter>> _loadOrder;
        private readonly SkyrimMod _outputMod;
        private readonly string _outputPath;
        private readonly IEnumerable<ModKey> _plugins;
        
        public HashSet<FormKey>? BrokenKeys { get; set; }
        
        public Merger(string dataFolderPath, List<ModKey> plugins, ModKey outputKey)
        {
            _loadOrder = LoadOrder.Import(
                dataFolderPath,
                plugins, 
                path => ModInstantiator<ISkyrimModGetter>.Importer(path, GameRelease.SkyrimSE));

            _outputMod = new SkyrimMod(outputKey, SkyrimRelease.SkyrimSE);
            _outputPath = Path.Combine(dataFolderPath, outputKey.FileName);
            _plugins = plugins;
        }
        
        public void Merge()
        {
            _loadOrder
                .PriorityOrder
                .Resolve()
                .MergeMods<ISkyrimModGetter, ISkyrimMod, ISkyrimMajorRecord, ISkyrimMajorRecordGetter>(_outputMod, out var brokenKeys);
            BrokenKeys = brokenKeys;
        }

        public void Dispose()
        {
            _loadOrder.Dispose();
            _outputMod.WriteToBinary(_outputPath, Utils.SafeBinaryWriteParameters);
        }
    }
}
