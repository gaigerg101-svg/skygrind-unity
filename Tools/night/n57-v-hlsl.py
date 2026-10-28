# NIGHT57 builder V: a shader syntax check without the Editor (builders never run Unity). Compiles the HLSL between HLSLPROGRAM and ENDHLSL of a
# blit shader (Unity's Blit.hlsl plumbing: _BlitTexture, Varyings, SAMPLE_TEXTURE2D_X_LOD stubbed; #pragma / #include lines dropped) with the
# Editor's own D3DCompiler_47.dll, target ps_5_0, entry Frag. It catches syntax and type errors in the shader's own code, not an error inside
# URP's includes or a platform other than D3D11.
#   python -I Tools/night/n57-v-hlsl.py <shader> [--dll <D3DCompiler_47.dll>]
#   python -I Tools/night/n57-v-hlsl.py planted     (the shader Cap57 as committed: OK; the same with one ';' removed: FAILED)
import sys, os, ctypes
from ctypes import c_void_p, c_char_p, c_size_t, c_uint, POINTER, byref

DLL = r"C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Data\Tools\D3DCompiler_47.dll"
STUB = """
Texture2D _BlitTexture; SamplerState sampler_LinearClamp; SamplerState sampler_PointClamp;
struct Varyings { float4 positionCS : SV_POSITION; float2 texcoord : TEXCOORD0; };
#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
#define SAMPLE_TEXTURE2D_X_LOD(t, s, uv, l) t.SampleLevel(s, uv, l)
"""


def body_of(src):
    b = src[src.index("HLSLPROGRAM") + len("HLSLPROGRAM"):src.index("ENDHLSL")]
    return "\n".join(l for l in b.splitlines() if not l.strip().startswith(("#pragma", "#include")))


def compile_hlsl(src, dll_path):
    code = (STUB + body_of(src)).encode("utf-8")
    dll = ctypes.WinDLL(dll_path)
    f = dll.D3DCompile
    f.argtypes = [c_void_p, c_size_t, c_char_p, c_void_p, c_void_p, c_char_p, c_char_p, c_uint, c_uint, POINTER(c_void_p), POINTER(c_void_p)]
    f.restype = ctypes.c_long
    blob, err = c_void_p(), c_void_p()
    hr = f(code, len(code), b"shader", None, None, b"Frag", b"ps_5_0", 0, 0, byref(blob), byref(err))
    text = ""
    if err.value:
        vt = ctypes.cast(ctypes.cast(err, POINTER(c_void_p))[0], POINTER(c_void_p))
        ptr = ctypes.WINFUNCTYPE(c_void_p, c_void_p)(vt[3])(err)
        size = ctypes.WINFUNCTYPE(c_size_t, c_void_p)(vt[4])(err)
        text = ctypes.string_at(ptr, size).decode("utf-8", "replace")
    return hr >= 0, text


def main():
    args = sys.argv[1:]
    dll = DLL
    if "--dll" in args:
        i = args.index("--dll"); dll = args[i + 1]; del args[i:i + 2]
    if not args:
        print("usage: n57-v-hlsl.py <shader> | planted"); sys.exit(2)
    if args[0] == "planted":
        here = os.path.dirname(os.path.abspath(__file__))
        src = open(os.path.join(here, "..", "..", "Assets", "Shaders", "Resources", "Cap57.shader"), encoding="utf-8").read()
        ok1, _ = compile_hlsl(src, dll)
        ok2, t2 = compile_hlsl(src.replace("float st = s;", "float st = s", 1), dll)
        print("planted Cap57 as committed: %s (want OK)" % ("OK" if ok1 else "FAILED"))
        print("planted Cap57 with one ';' removed: %s (want FAILED) %s" % ("OK" if ok2 else "FAILED", t2.strip().splitlines()[0] if t2.strip() else ""))
        sys.exit(0 if ok1 and not ok2 else 1)
    ok, text = compile_hlsl(open(args[0], encoding="utf-8").read(), dll)
    print("%s: %s" % (args[0], "OK (ps_5_0, Frag)" if ok else "FAILED"))
    if text.strip():
        print(text.strip())
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
