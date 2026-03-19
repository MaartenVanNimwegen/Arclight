using System;
using System.Collections.Generic;
using System.Text;

namespace Arclight.Application.Interfaces
{
    public interface ISlugService
    {
        Task<string> GenerateUniqueSlugAsync(string title);    
    }
}
