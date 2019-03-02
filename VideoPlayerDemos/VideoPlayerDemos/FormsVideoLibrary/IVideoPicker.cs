using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FormsVideoLibrary
{
    public interface IVideoPicker
    {
        Task<KeyValuePair<List<string>, int>?> GetVideoFileAsync();
    }
}
