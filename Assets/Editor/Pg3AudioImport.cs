// PG3: the import settings of the ElevenLabs audio. The music tracks (60-170 s) stream from disk as Vorbis, so 21 tracks don't sit
// decompressed in memory (about 20 MB each as PCM); the short effects decompress on load so a shot starts with no latency.
using UnityEditor;
using UnityEngine;

namespace Skygrind.EditorTools
{
    public sealed class Pg3AudioImport : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            string p = assetPath.Replace('\\', '/');
            if (!p.Contains("/Resources/Audio/PG3/")) return;
            var imp = (AudioImporter)assetImporter;
            AudioImporterSampleSettings s = imp.defaultSampleSettings;
            bool music = p.Contains("/Audio/PG3/Music/");
            s.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = music ? 0.7f : 0.85f;
            imp.defaultSampleSettings = s;
            imp.loadInBackground = music;
        }
    }
}
