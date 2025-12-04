using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Fonts;

namespace CafeAvalonia.Services
{
    public static class PdfConfig
    {
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;
            GlobalFontSettings.FontResolver = new SingleFontResolver();
            _initialized = true;
        }
    }
}
