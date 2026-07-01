using System;
using System.Runtime.InteropServices;

class SetDefaultMic
{
    [DllImport("ole32.dll")]
    static extern int CoCreateInstance(
        [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
        IntPtr outer, uint clsContext,
        [MarshalAs(UnmanagedType.LPStruct)] Guid iid,
        out IntPtr ppv);

    [DllImport("ole32.dll")]
    static extern void CoTaskMemFree(IntPtr p);

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(int dataFlow, uint mask, out IntPtr ppDevices);
        int GetDefaultAudioEndpoint(int dataFlow, int role, out IntPtr ppDev);
        int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IntPtr ppDev);
    }

    [ComImport, Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceCollection
    {
        int GetCount(out uint c);
        int Item(uint n, out IntPtr ppDev);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        int Activate([MarshalAs(UnmanagedType.LPStruct)] Guid iid, uint ctx, IntPtr p, out IntPtr pp);
        int OpenPropertyStore(uint access, out IntPtr ppProps);
        int GetId(out IntPtr ppId);
    }

    [ComImport, Guid("F8679F50-850A-41CF-9C72-430F290290C8"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfig
    {
        int GetMixFormat(string deviceId, out IntPtr ppFormat);
        int SetDefaultEndpoint(string deviceId, int role);
    }

    [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        int GetValue(ref PROPERTYKEY key, out PROPVARIANT v);
        int SetValue(ref PROPERTYKEY key, ref PROPVARIANT v);
        int Commit();
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct PROPVARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(8)] public IntPtr pwszVal;
    }

    static void Main(string[] args)
    {
        string target = args.Length > 0 ? args[0] : "英特尔";
        Guid CLSID_MMDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
        Guid IID_IMMDeviceEnumerator = new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6");
        Guid CLSID_PolicyConfig = new Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9");
        Guid IID_IPolicyConfig = new Guid("F8679F50-850A-41CF-9C72-430F290290C8");
        PROPERTYKEY PKEY_FriendlyName = new PROPERTYKEY
        {
            fmtid = new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"),
            pid = 14
        };

        IntPtr pEnum;
        CoCreateInstance(CLSID_MMDeviceEnumerator, IntPtr.Zero, 1,
            IID_IMMDeviceEnumerator, out pEnum);
        IMMDeviceEnumerator en =
            (IMMDeviceEnumerator)Marshal.GetObjectForIUnknown(pEnum);

        IntPtr pDevs;
        en.EnumAudioEndpoints(1, 1, out pDevs); // eCapture, ACTIVE
        IMMDeviceCollection coll =
            (IMMDeviceCollection)Marshal.GetObjectForIUnknown(pDevs);
        coll.GetCount(out uint cnt);

        string foundId = null;
        for (uint i = 0; i < cnt; i++)
        {
            coll.Item(i, out IntPtr pDev);
            IMMDevice dev = (IMMDevice)Marshal.GetObjectForIUnknown(pDev);
            dev.OpenPropertyStore(0, out IntPtr pProps);
            IPropertyStore ps = (IPropertyStore)Marshal.GetObjectForIUnknown(pProps);
            PROPVARIANT pv = default;
            ps.GetValue(ref PKEY_FriendlyName, out pv);
            string name = Marshal.PtrToStringUni(pv.pwszVal);
            dev.GetId(out IntPtr pId);
            string id = Marshal.PtrToStringUni(pId);
            Console.WriteLine("  [{0}] {1}", i, name);
            if (name.Contains(target)) foundId = id;
            Marshal.ReleaseComObject(dev);
        }

        if (foundId != null)
        {
            IntPtr pPol;
            CoCreateInstance(CLSID_PolicyConfig, IntPtr.Zero, 1,
                IID_IPolicyConfig, out pPol);
            IPolicyConfig pc =
                (IPolicyConfig)Marshal.GetObjectForIUnknown(pPol);
            pc.SetDefaultEndpoint(foundId, 0); // eConsole
            pc.SetDefaultEndpoint(foundId, 1); // eCommunications
            Console.WriteLine("SUCCESS: switched default mic to " + target);
        }
        else
        {
            Console.WriteLine("ERROR: target device not found");
        }
        Marshal.ReleaseComObject(en);
    }
}
