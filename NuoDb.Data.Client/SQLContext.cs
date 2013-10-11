/****************************************************************************
* Copyright (c) 2012-2013, NuoDB, Inc.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*   * Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*   * Redistributions in binary form must reproduce the above copyright
*     notice, this list of conditions and the following disclaimer in the
*     documentation and/or other materials provided with the distribution.
*   * Neither the name of NuoDB, Inc. nor the names of its contributors may
*     be used to endorse or promote products derived from this software
*     without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL NUODB, INC. BE LIABLE FOR ANY DIRECT, INDIRECT,
* INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
* LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
* OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
* OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
* ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

namespace NuoDb.Data.Client
{
    static class OlsonDatabase
    {
        private static Dictionary<string, string> db = new Dictionary<string, string>();

        static OlsonDatabase()
        {
            // data for the mapping between TZ names and Windows time zones 
            // has been taken from http://unicode.org/repos/cldr/trunk/common/supplemental/windowsZones.xml
            XmlReader reader = XmlReader.Create(typeof(SQLContext).Assembly.GetManifestResourceStream("NuoDb.Data.Client.TimeZones.xml"));
            while(reader.ReadToFollowing("mapZone"))
            {
                reader.MoveToFirstAttribute();
                string windowsZone = reader.Value;
                reader.MoveToNextAttribute();
                string territory = reader.Value;
                reader.MoveToNextAttribute();
                string olsonZones = reader.Value;
                string[] timezones = olsonZones.Split(' ');
                // map the Windows time zone to the first choice in the TZ names
                db.Add(windowsZone + "|" + territory, timezones[0]);
                // map all of the TZ names to the same Windows time zone
                foreach(string tz in timezones)
                    if(!db.ContainsKey(tz))
                        db.Add(tz, windowsZone);
            }
        }

        public static TimeZoneInfo FindWindowsTimeZone(string OlsonTimeZone)
        {
            string WindowsZone;
            if (db.TryGetValue(OlsonTimeZone, out WindowsZone))
                return TimeZoneInfo.FindSystemTimeZoneById(WindowsZone);

            throw new TimeZoneNotFoundException("Unknown timezone '" + OlsonTimeZone + "'");
        }

        public static string FindOlsonTimeZone(string WindowsTimeZone)
        {
            // extract the country code from the culture name (e.g. en-US)
            string[] parts = CultureInfo.CurrentCulture.Name.Split('-');
            string TimezoneKey, OlsonZone;
            if (parts.Length >= 2)
            {
                TimezoneKey = WindowsTimeZone + "|" + parts[1];
                if (db.TryGetValue(TimezoneKey, out OlsonZone))
                    return OlsonZone;
            }
            // back-up: use the generic country code 001
            TimezoneKey = WindowsTimeZone + "|001";
            if (db.TryGetValue(TimezoneKey, out OlsonZone))
                return OlsonZone;

            throw new TimeZoneNotFoundException("Unknown timezone '" + WindowsTimeZone + "'");
        }
    }

    class SQLContext
    {
        private TimeZoneInfo timeZone;

        public TimeZoneInfo TimeZone
        {
            get { return timeZone; }
            set { timeZone = value; }
        }
    }
}
