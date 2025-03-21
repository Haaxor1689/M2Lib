﻿using M2Lib.m2;

namespace M2Lib.interfaces
{
    /// <summary>
    ///     Handles the content pointed by the M2Array objects in the structure.
    /// </summary>
    public interface IReferencer : IMarshalable
    {
        void LoadContent(BinaryReader stream, M2.Format version);
        void SaveContent(BinaryWriter stream, M2.Format version);
    }
}
