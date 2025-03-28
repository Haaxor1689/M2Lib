﻿using System.Diagnostics;
using System.Text;
using M2Lib.interfaces;
using M2Lib.types;

namespace M2Lib.m2
{
    public class M2SkinProfile : IReferencer
    {
        public M2Array<ushort> Indices { get; set; } = [];
        public M2Array<ushort> Triangles { get; set; } = [];
        public M2Array<VertexProperty> Properties { get; set; } = [];
        public M2Array<M2SkinSection> Submeshes { get; set; } = [];
        public M2Array<M2Batch> TextureUnits { get; set; } = [];
        public uint Bones { get; set; } = 21;
        public M2Array<M2ShadowBatch> ShadowBatches { get; set; } = [];

        public void Load(BinaryReader stream, M2.Format version)
        {
            if (version >= M2.Format.LichKing)
            {
                var magic = Encoding.UTF8.GetString(stream.ReadBytes(4));
                Debug.Assert(magic == "SKIN");
            }
            Indices.Load(stream, version);
            Triangles.Load(stream, version);
            Properties.Load(stream, version);
            Submeshes.Load(stream, version);
            TextureUnits.Load(stream, version);
            Bones = stream.ReadUInt32();
            if (version >= M2.Format.Cataclysm)
                ShadowBatches.Load(stream, version);
        }

        public void Save(BinaryWriter stream, M2.Format version)
        {
            if (version >= M2.Format.LichKing)
                stream.Write(Encoding.UTF8.GetBytes("SKIN"));
            Indices.Save(stream, version);
            Triangles.Save(stream, version);
            Properties.Save(stream, version);
            Submeshes.Save(stream, version);
            TextureUnits.Save(stream, version);
            stream.Write(Bones);
            if (version >= M2.Format.Cataclysm)
                ShadowBatches.Save(stream, version);
        }

        public void LoadContent(BinaryReader stream, M2.Format version)
        {
            Indices.LoadContent(stream, version);
            Triangles.LoadContent(stream, version);
            Properties.LoadContent(stream, version);
            Submeshes.LoadContent(stream, version);
            TextureUnits.LoadContent(stream, version);
            if (version >= M2.Format.Cataclysm)
                ShadowBatches.LoadContent(stream, version);
        }

        public void SaveContent(BinaryWriter stream, M2.Format version)
        {
            Indices.SaveContent(stream, version);
            Triangles.SaveContent(stream, version);
            Properties.SaveContent(stream, version);
            Submeshes.SaveContent(stream, version);
            TextureUnits.SaveContent(stream, version);
            if (version >= M2.Format.Cataclysm)
                ShadowBatches.SaveContent(stream, version);
        }

        public static string SkinFileName(string path, int number)
        {
            var pathDirectory = Path.GetDirectoryName(path);
            Debug.Assert(pathDirectory != null, "pathDirectory != null");
            var pathWithoutExt = Path.GetFileNameWithoutExtension(path);
            string animFileName = $"{pathWithoutExt}{number:00}.skin";
            return Path.Combine(pathDirectory, animFileName);
        }
    }
}
