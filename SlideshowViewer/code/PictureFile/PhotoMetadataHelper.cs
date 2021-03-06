﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace SlideshowViewer.PictureFile
{
    public static class FILETIMEExtensions
    {
        public static DateTime ToDateTime(this FILETIME filetime)
        {
            byte[] bytes = Utils.Concat(BitConverter.GetBytes(filetime.dwLowDateTime),
                                        BitConverter.GetBytes(filetime.dwHighDateTime));
            return DateTime.FromFileTime(BitConverter.ToInt64(bytes, 0));
        }
    }

    internal class IIDStrings // Defined in PropSys.h
    {
        public const string IPropertyDescriptionList = "1f9fc1d0-c39b-4b26-817f-011967d3440e";
        public const string IPropertyDescription = "6f79d558-3e96-4549-a1d1-7d75d2288814";
        public const string IPropertyDescriptionSearchInfo = "27b66c33-6612-4172-a19a-97fafe115e30";
    }

    [ComImport,
     Guid(IIDStrings.IPropertyDescriptionList),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyDescriptionList
    {
        // HRESULT GetCount(/* [out] */ UINT *pcElem);		
        uint GetCount();

        // HRESULT GetAt(/*[in]*/ UINT iElem, /*[in]*/ REFIID riid, /*[iid_is][out]*/ void **ppv);
        [return: MarshalAs(UnmanagedType.Interface)]
        IPropertyDescription GetAt(uint Index, ref Guid riid);
    }

    [ComImport,
     Guid(IIDStrings.IPropertyDescription),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyDescription
    {
        //// HRESULT GetPropertyKey(/*[out]*/ PROPERTYKEY *pkey);
        void NotImplemented1();

        //// HRESULT GetCanonicalName(/*[out]*/ LPWSTR *ppszName);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetCanonicalName();

        //// HRESULT GetPropertyType(/*[out]*/ VARTYPE *pvartype);
        uint GetPropertyType();

        //// HRESULT GetDisplayName(/*[out]*/ LPWSTR *ppszName);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetDisplayName();

        //// HRESULT GetEditInvitation(/*[out]*/ LPWSTR *ppszInvite);
        void NotImplemented2();

        //// HRESULT GetTypeFlags(/*[in]*/ PROPDESC_TYPE_FLAGS mask, /*[out]*/ *ppdtFlags);
        uint GetTypeFlags(PropDescTypeFlags Mask);
    }

    [Flags]
    internal enum PropDescTypeFlags : uint
    {
        PDTF_MULTIPLEVALUES = 0x00000001, //can have multiple values. 
        PDTF_ISVIEWABLE = 0x00000080, // property is meant to be viewed by the user
        PDTF_MASK_ALL = 0x800001FF, // Mask for all flags.
    }

    [ComImport,
     Guid(IIDStrings.IPropertyDescriptionSearchInfo),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyDescriptionSearchInfo
    {
        ////HRESULT GetSearchInfoFlags(/*[out]*/PROPDESC_SEARCHINFO_FLAGS *ppdsiFlags);
        PROPDESC_SEARCHINFO_FLAGS GetSearchInfoFlags();
    }

    [Flags]
    internal enum PROPDESC_SEARCHINFO_FLAGS
    {
        PDSIF_ISCOLUMN = 0x2,
    }

    internal class PhotoMetadataHelper
    {
        // Defined in PropSys.h
        // PSSTDAPI PSEnumeratePropertyDescriptions(PROPDESC_ENUMFILTER filterOn, REFIID riid, VOID** ppv);
        [DllImport("propsys.dll")]
        public static extern int PSEnumeratePropertyDescriptions(
            int filterOn,
            [In] ref Guid riid,
            out IPropertyDescriptionList ppv);

        public static IEnumerable<string> GetPropertyDefinitions()
        {
            Guid riid;
            IPropertyDescriptionList propList = null;
            IPropertyDescription propDesc = null;

            try
            {
                riid = new Guid(IIDStrings.IPropertyDescriptionList);
                int hr = PSEnumeratePropertyDescriptions(0, // PDEF_ALL,
                                                         ref riid,
                                                         out propList);

                if (hr < 0)
                {
                    throw (new ApplicationException(string.Format("PSEnumeratePropertyDescriptions returned: {0}", hr)));
                }

                uint numProps = propList.GetCount();
                riid = new Guid(IIDStrings.IPropertyDescription);

                for (int i = 0; i < numProps; i++)
                {
                    // Iterates through each item in the description list 
                    propDesc = propList.GetAt((uint) i, ref riid);

                    yield return propDesc.GetCanonicalName();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(propList);
                Marshal.ReleaseComObject(propDesc);
            }
        }


    }
}