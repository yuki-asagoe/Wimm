using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Model.Control;
using Wimm.Ui.Records;

namespace Wimm.Ui.ViewModel
{
    public class RegenerateFolderViewModel : IDisposable
    {
        public GeneratorEntry[] GeneratorEntries { get; }
        MachineFolder.Generator? Generator { get; }
        
        public RegenerateFolderViewModel(DirectoryInfo machineFolder)
        {
            Generator = new MachineFolder.Generator(machineFolder);
            GeneratorEntries = new GeneratorEntry[] {
                new GeneratorEntry("description",()=>Generator.GenerateDescription()),
                new GeneratorEntry("config",()=>Generator.GenerateConfig()),
                new GeneratorEntry("docs",()=>Generator.GenerateDocs()),
                new GeneratorEntry("script",()=>Generator.GenerateScript()),
                new GeneratorEntry("meta info",()=>Generator.GenerateMetaInfo())
            };
        }
        private bool disposed = false;
        public void Dispose()
        {
            if (disposed) return;
            Generator?.Dispose();
            GC.SuppressFinalize(this);
            disposed = true;
        }
        ~RegenerateFolderViewModel()
        {
            Generator?.Dispose();
        }
    }
}
