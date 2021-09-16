﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NeroNet.Classes
{
    public class IconInjector
    {
        [SuppressUnmanagedCodeSecurity()]

        private class NativeMethods
        {
            [DllImport("kernel32")]
            public static extern IntPtr BeginUpdateResource(string fileName, [MarshalAs(UnmanagedType.Bool)] bool deleteExistingResources);
            [DllImport("kernel32")]
            public static extern bool UpdateResource(IntPtr hUpdate, IntPtr type, IntPtr name, short language, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] data, int dataSize);
            [DllImport("kernel32")]
            public static extern bool EndUpdateResource(IntPtr hUpdate, [MarshalAs(UnmanagedType.Bool)] bool discard);
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct ICONDIR
        {
            public ushort Reserved;
            public ushort Type;
            public ushort Count;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONDIRENTRY
        {
            public byte Width;
            public byte Height;
            public byte ColorCount;
            public byte Reserved;
            public ushort Planes;
            public ushort BitCount;
            public int BytesInRes;
            public int ImageOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint Size;
            public int Width;
            public int Height;
            public ushort Planes;
            public ushort BitCount;
            public uint Compression;
            public uint SizeImage;
            public int XPelsPerMeter;
            public int YPelsPerMeter;
            public uint ClrUsed;
            public uint ClrImportant;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct GRPICONDIRENTRY
        {
            public byte Width;
            public byte Height;
            public byte ColorCount;
            public byte Reserved;
            public ushort Planes;
            public ushort BitCount;
            public int BytesInRes;
            public ushort ID;
        }
        public static void InjectIcon(string exeFileName, string iconFileName)
        {
            InjectIcon(exeFileName, iconFileName, 0, 0);
        }

        public static void InjectIcon(string exeFileName, string iconFileName, uint iconGroupID, uint iconBaseID)
        {
            const int RT_ICON = (int)3U;
            const int RT_GROUP_ICON = (int)14U;
            IconFile iconFile = IconFile.FromFile(iconFileName);
            var hUpdate = NativeMethods.BeginUpdateResource(exeFileName, false);
            var data = iconFile.CreateIconGroupData(iconBaseID);
            NativeMethods.UpdateResource(hUpdate, new IntPtr(RT_GROUP_ICON), new IntPtr(iconGroupID), 0, data, data.Length);
            for (var i = 0; i <= iconFile.ImageCount - 1; i++)
            {
                byte[] image = iconFile.ImageData(i);
                NativeMethods.UpdateResource(hUpdate, new IntPtr(RT_ICON), new IntPtr(iconBaseID + i), 0, image, image.Length);
            }
            NativeMethods.EndUpdateResource(hUpdate, false);
        }
        private class IconFile
        {
            private ICONDIR iconDir = new ICONDIR();
            private ICONDIRENTRY[] iconEntry;
            private byte[][] iconImage;
            public int ImageCount
            {
                get
                {
                    return iconDir.Count;
                }
            }
            public byte[] ImageData(int index)
            {
                return iconImage[index];
            }
            public static IconFile FromFile(string filename)
            {
                IconFile instance = new IconFile();
                byte[] fileBytes = System.IO.File.ReadAllBytes(filename);

                GCHandle pinnedBytes = GCHandle.Alloc(fileBytes, GCHandleType.Pinned);
                instance.iconDir = (ICONDIR)Marshal.PtrToStructure(pinnedBytes.AddrOfPinnedObject(), typeof(ICONDIR));

                instance.iconEntry = new ICONDIRENTRY[instance.iconDir.Count - 1 + 1];
                instance.iconImage = new byte[instance.iconDir.Count - 1 + 1][];

                var offset = Marshal.SizeOf(instance.iconDir);

                var iconDirEntryType = typeof(ICONDIRENTRY);
                var size = Marshal.SizeOf(iconDirEntryType);
                for (var i = 0; i <= instance.iconDir.Count - 1; i++)
                {
                    var entry = (ICONDIRENTRY)Marshal.PtrToStructure(new IntPtr(pinnedBytes.AddrOfPinnedObject().ToInt64() + offset), iconDirEntryType);
                    instance.iconEntry[i] = entry;
                    instance.iconImage[i] = new byte[entry.BytesInRes - 1 + 1];
                    Buffer.BlockCopy(fileBytes, entry.ImageOffset, instance.iconImage[i], 0, entry.BytesInRes);
                    offset += size;
                }
                pinnedBytes.Free();
                return instance;
            }
            public byte[] CreateIconGroupData(uint iconBaseID)
            {
                int sizeOfIconGroupData = Marshal.SizeOf(typeof(ICONDIR)) + Marshal.SizeOf(typeof(GRPICONDIRENTRY)) * ImageCount;
                byte[] data = new byte[sizeOfIconGroupData - 1 + 1];
                var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
                Marshal.StructureToPtr(iconDir, pinnedData.AddrOfPinnedObject(), false);
                var offset = Marshal.SizeOf(iconDir);
                for (var i = 0; i <= ImageCount - 1; i++)
                {
                    GRPICONDIRENTRY grpEntry = new GRPICONDIRENTRY();
                    BITMAPINFOHEADER bitmapheader = new BITMAPINFOHEADER();
                    var pinnedBitmapInfoHeader = GCHandle.Alloc(bitmapheader, GCHandleType.Pinned);
                    Marshal.Copy(ImageData(i), 0, pinnedBitmapInfoHeader.AddrOfPinnedObject(), Marshal.SizeOf(typeof(BITMAPINFOHEADER)));
                    pinnedBitmapInfoHeader.Free();
                    grpEntry.Width = iconEntry[i].Width;
                    grpEntry.Height = iconEntry[i].Height;
                    grpEntry.ColorCount = iconEntry[i].ColorCount;
                    grpEntry.Reserved = iconEntry[i].Reserved;
                    grpEntry.Planes = bitmapheader.Planes;
                    grpEntry.BitCount = bitmapheader.BitCount;
                    grpEntry.BytesInRes = iconEntry[i].BytesInRes;
                    grpEntry.ID = System.Convert.ToUInt16(iconBaseID + i);
                    Marshal.StructureToPtr(grpEntry, new IntPtr(pinnedData.AddrOfPinnedObject().ToInt64() + offset), false);
                    offset += Marshal.SizeOf(typeof(GRPICONDIRENTRY));
                }
                pinnedData.Free();
                return data;
            }
        }
    }
}