using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Collections.Generic;
using Android.Provider;
using Android.Database;

namespace ClipBrowser.Droid
{
    public static class AndroidNetUriToPath
    {
        public static String GetPath(Context context, Android.Net.Uri uri)
        {
            // DocumentProvider
            if (DocumentsContract.IsDocumentUri(context, uri))
            {
                // ExternalStorageProvider
                if ("com.android.externalstorage.documents".Equals(uri.Authority, StringComparison.OrdinalIgnoreCase))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(":");
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, split[1]);
                    }
                    // TODO handle non-primary volumes
                }
                // DownloadsProvider
                else if ("com.android.providers.downloads.documents".Equals(uri.Authority, StringComparison.OrdinalIgnoreCase))
                {
                    string id = DocumentsContract.GetDocumentId(uri);
                    var contentUri = ContentUris.WithAppendedId(Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return GetDataColumn(context, contentUri, null, null);
                }
                // MediaProvider
                else if ("com.android.providers.media.documents".Equals(uri.Authority, StringComparison.OrdinalIgnoreCase))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(":");
                    string type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type) || "video".Equals(type) || "audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    string selection = "_id=?";
                    string[] selectionArgs = new String[] { split[1] };

                    return GetDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return GetDataColumn(context, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        /**
         * Get the value of the data column for this Uri. This is useful for
         * MediaStore Uris, and other file-based ContentProviders.
         * 
         * @param context
         *            The context.
         * @param uri
         *            The Uri to query.
         * @param selection
         *            (Optional) Filter used in the query.
         * @param selectionArgs
         *            (Optional) Selection arguments used in the query.
         * @return The value of the _data column, which is typically a file path.
         */
        public static String GetDataColumn(Context context, Android.Net.Uri uri, String selection,
                String[] selectionArgs)
        {

            ICursor cursor = null;
            string column = "_data";
            string[] projection = { column };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int column_index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(column_index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }
    }    
}
