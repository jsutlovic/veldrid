using NativeLibraryLoader;
using System;
using System.Runtime.InteropServices;

namespace Veldrid
{
    public unsafe class RenderDoc
    {
        private readonly RENDERDOC_API_1_3_0 _api;
        private NativeLibrary _nativeLib;

        private unsafe RenderDoc(NativeLibrary lib)
        {
            _nativeLib = lib;
            pRENDERDOC_GetAPI getApiFunc = _nativeLib.LoadFunction<pRENDERDOC_GetAPI>("RENDERDOC_GetAPI");
            void* apiPointers;
            int result = getApiFunc(RENDERDOC_Version.eRENDERDOC_API_Version_1_2_0, &apiPointers);
            if (result != 1)
            {
                throw new Exception("Failed to load RenderDoc API.");
            }

            _api = Marshal.PtrToStructure<RENDERDOC_API_1_3_0>((IntPtr)apiPointers);
        }

        public static bool Load(out RenderDoc renderDoc) => Load(GetLibName(), out renderDoc);

        public static bool Load(string renderDocLibPath, out RenderDoc renderDoc)
        {
            try
            {
                NativeLibrary lib = new NativeLibrary(renderDocLibPath);
                renderDoc = new RenderDoc(lib);
                return true;
            }
            catch
            {
                renderDoc = null;
                return false;
            }
        }

        private static string GetLibName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "renderdoc.dll";
            }
            else
            {
                return "librenderdoc.so";
            }
        }

        public void TriggerCapture() => _api.TriggerCapture();
        public void StartFrameCapture() => _api.StartFrameCapture(null, null);
        public bool IsFrameCapturing() => _api.IsFrameCapturing() != 0;
        public bool EndFrameCapture() => _api.EndFrameCapture(null, null) != 0;
    }
}
