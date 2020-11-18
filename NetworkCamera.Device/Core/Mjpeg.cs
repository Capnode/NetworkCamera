/*
 * Copyright 2019 Capnode AB
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

//
// Copyright © Andrew Kirillov, 2005-2006
// andrew.kirillov@gmail.com

namespace NetworkCamera.Device.Core
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Net;
    using System.Globalization;
    using Serilog;
    using System.Diagnostics;

    internal class Mjpeg : IDevice
    {
        private object _userData;
        private bool _useSeparateConnectionGroup = true;

        private const int _bufSize = 512 * 1024; // buffer size
        private const int _readSize = 1024;    // portion size to read

        // SeparateConnectioGroup property
        // indicates to open WebRequest in separate connection group
        public bool SeparateConnectionGroup
        {
            get { return _useSeparateConnectionGroup; }
            set { _useSeparateConnectionGroup = value; }
        }

        public long FramesReceived { get; set; }

        public long BytesReceived {get; set; }

        public object UserData
        {
            get { return _userData; }
            set { _userData = value; }
        }

        public void Main(DeviceModel device, DeviceEventHandler deviceEvent, CancellationToken token)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrWhiteSpace(device.Source)) throw new ArgumentException(nameof(device.Source));
            if (deviceEvent == null) throw new ArgumentNullException(nameof(deviceEvent));
            if (token == null) throw new ArgumentNullException(nameof(token));

            try
            {
                Run(device, deviceEvent, token);
            }
            finally
            {
                // notify client
                deviceEvent(this, new DeviceEventArgs(null));
            }
        }

        private void Run(DeviceModel device, DeviceEventHandler deviceEvent, CancellationToken token)
        {
            byte[] buffer = new byte[_bufSize];  // buffer to read stream
            while (!token.IsCancellationRequested)
            {
                byte[] delimiter = null;
                byte[] delimiter2 = null;
                byte[] boundary = null;
                int boundaryLen;
                int delimiterLen = 0;
                int delimiter2Len = 0;
                int read;
                int todo = 0;
                int total = 0;
                int pos = 0;
                int align = 1;
                int start = 0;
                int stop = 0;

                // align
                //  1 = searching for image start
                //  2 = searching for image end
                // create request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(device.Source));

                // set login and password
                if ((device.Login != null) && (device.Password != null) && (!string.IsNullOrEmpty(device.Login)))
                {
                    request.Credentials = new NetworkCredential(device.Login, device.Password);
                }

                // set connection group name
                if (_useSeparateConnectionGroup)
                {                        request.ConnectionGroupName = GetHashCode().ToString(CultureInfo.InvariantCulture);
                }

                // get response
                using WebResponse resp = request.GetResponse();

                // check content type
                string ct = resp.ContentType;
                if (!ct.Contains("multipart/x-mixed-replace"))
                {
                    throw new ApplicationException($"Invalid content type: {ct}");
                }

                // get boundary
                ASCIIEncoding encoding = new ASCIIEncoding();
                boundary = encoding.GetBytes(ct.Substring(ct.IndexOf("boundary=", 0, StringComparison.InvariantCultureIgnoreCase) + 9));
                boundaryLen = boundary.Length;

                // get response stream
                using Stream stream = resp.GetResponseStream();

                // loop
                while (!token.IsCancellationRequested)
                {
                    // check total read
                    if (total > _bufSize - _readSize)
                    {
                        total = pos = todo = 0;
                    }

                    // read next portion from stream
                    if ((read = stream.Read(buffer, total, _readSize)) == 0) break;

                    total += read;
                    todo += read;

                    // increment received bytes counter
                    BytesReceived += read;

                    // does we know the delimiter ?
                    if (delimiter == null)
                    {
                        // find boundary
                        pos = ByteArrayUtils.Find(buffer, boundary, pos, todo);

                        if (pos == -1)
                        {
                            // was not found
                            todo = boundaryLen - 1;
                            pos = total - todo;
                            continue;
                        }

                        Debug.Assert(pos >= 0);
                        todo = total - pos;

                        if (todo < 2) continue;

                        // check new line delimiter type
                        if (buffer[pos + boundaryLen] == 10)
                        {
                            delimiterLen = 2;
                            delimiter = new byte[2] { 10, 10 };
                            delimiter2Len = 1;
                            delimiter2 = new byte[1] { 10 };
                        }
                        else
                        {
                            delimiterLen = 4;
                            delimiter = new byte[4] { 13, 10, 13, 10 };
                            delimiter2Len = 2;
                            delimiter2 = new byte[2] { 13, 10 };
                        }

                        pos += boundaryLen + delimiter2Len;
                        todo = total - pos;
                    }

                    if (pos < 0) continue;

                    // search for image
                    if (align == 1)
                    {
                        start = ByteArrayUtils.Find(buffer, delimiter, pos, todo);
                        if (start != -1)
                        {
                            // found delimiter
                            start += delimiterLen;
                            pos = start;
                            todo = total - pos;
                            align = 2;
                        }
                        else
                        {
                            // delimiter not found
                            todo = delimiterLen - 1;
                            pos = total - todo;
                        }
                    }

                    // search for image end
                    while ((align == 2) && (todo >= boundaryLen))
                    {
                        stop = ByteArrayUtils.Find(buffer, boundary, pos, todo);
                        if (stop != -1)
                        {
                            pos = stop;
                            todo = total - pos;

                            // increment frames counter
                            FramesReceived++;

                            // image at stop
                            using var ms = new MemoryStream(buffer, start, stop - start);
                            Bitmap bitmap = System.Drawing.Image.FromStream(ms) as Bitmap;

                            // notify client
                            deviceEvent(this, new DeviceEventArgs(bitmap));

                            // release the image
                            bitmap.Dispose();
                            bitmap = null;

                            // shift array
                            pos = stop + boundaryLen;
                            todo = total - pos;
                            Array.Copy(buffer, pos, buffer, 0, todo);

                            total = todo;
                            pos = 0;
                            align = 1;
                        }
                        else
                        {
                            // delimiter not found
                            todo = boundaryLen - 1;
                            pos = total - todo;
                        }
                    }
                }
            }
        }
    }
}
