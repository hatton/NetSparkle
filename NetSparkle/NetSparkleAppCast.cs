﻿using System;
using System.IO;
using System.Xml;
using System.Net;

namespace NetSparkle
{
    /// <summary>
    /// An app-cast 
    /// </summary>
    public class NetSparkleAppCast
    {
        private readonly NetSparkleConfiguration _config;
        private readonly String _castUrl;

        private const String itemNode = "item";
        private const String enclosureNode = "enclosure";
        private const String releaseNotesLinkNode = "sparkle:releaseNotesLink";
        private const String versionAttribute = "sparkle:version";
		private const String deltaFromAttribute = "sparkle:deltaFrom";
		private const String dasSignature = "sparkle:dsaSignature";
        private const String urlAttribute = "url";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="config">the current configuration</param>
        public NetSparkleAppCast(string castUrl, NetSparkleConfiguration config)
        {
            _config = config;
            _castUrl = castUrl;
        }

        /// <summary>
        /// Gets the latest version
        /// </summary>
        /// <returns>the AppCast item corresponding to the latest version</returns>
        public NetSparkleAppCastItem GetLatestVersion()
        {
            NetSparkleAppCastItem latestVersion = null;

            if (_castUrl.StartsWith("file://")) //handy for testing
            {
                var path = _castUrl.Replace("file://", "");
                using (var reader = XmlReader.Create(path))
                {
					latestVersion = ReadAppCast(reader, latestVersion, _config.InstalledVersion);
                }
            }
            else
            {
                // build a http web request stream
                WebRequest request = WebRequest.Create(_castUrl);
                request.UseDefaultCredentials = true;

                // request the cast and build the stream
                WebResponse response = request.GetResponse();
                using (Stream inputstream = response.GetResponseStream())
                {
                    using (XmlTextReader reader = new XmlTextReader(inputstream))
                    {
						latestVersion = ReadAppCast(reader, latestVersion, _config.InstalledVersion);
                    }
                }
            }

            latestVersion.AppName = _config.ApplicationName;
            latestVersion.AppVersionInstalled = _config.InstalledVersion;
            return latestVersion;
        }

        private static NetSparkleAppCastItem ReadAppCast(XmlReader reader,
                                                         NetSparkleAppCastItem latestVersion, string installedVersion)
        {
            NetSparkleAppCastItem currentItem = null;

			// The fourth segment of the version number is ignored by Windows Installer:
			var installedVersionV = new Version(installedVersion);
			var installedVersionWithoutFourthSegment = new Version(installedVersionV.Major, installedVersionV.Minor,
																   installedVersionV.Build);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
                            {
                                currentItem = new NetSparkleAppCastItem();
                                break;
                            }
                        case releaseNotesLinkNode:
                            {
                                currentItem.ReleaseNotesLink = reader.ReadString().Trim();
                                break;
                            }
                        case enclosureNode:
							{
								var deltaFrom = reader.GetAttribute(deltaFromAttribute);
								if (deltaFrom == null || deltaFrom == installedVersionWithoutFourthSegment.ToString())
								{
									currentItem.Version = reader.GetAttribute(versionAttribute);
									currentItem.DownloadLink = reader.GetAttribute(urlAttribute);
									currentItem.DSASignature = reader.GetAttribute(dasSignature);
								}
								break;
							}
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
                            {
                                if (latestVersion == null)
                                    latestVersion = currentItem;
                                else if (currentItem.CompareTo(latestVersion) > 0)
                                {
                                    latestVersion = currentItem;
                                }
                                break;
                            }
                    }
                }
            }
            return latestVersion;
        }
    }
}
