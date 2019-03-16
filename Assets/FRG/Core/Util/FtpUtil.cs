using System.Net;
using System.IO;
using System;
using UnityEngine;
using FRG.SharedCore;

public class FtpUtil
{
    public static void UploadFile(string remoteFilePath, string username, string password, byte[] data)
    {
        if (string.IsNullOrEmpty(remoteFilePath)|| string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || data == null || data.Length == 0)
        {
            Debug.LogError("Cannot upload file to ftp, host, username, password or file data is empty");
            return;
        }

        try
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFilePath);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(username, password);
            // Copy the contents of the file to the request stream.
            request.ContentLength = data.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Debug.Log("Upload File Complete, status " + response.StatusDescription);

            response.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error uploading file to FTP.\n" + ex.ToString());
        }
    }
}
