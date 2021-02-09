using ordercloud.integrations.exchangerates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Mappers
{
	public static class Geography
	{
        public static CurrencySymbol GetCurrency(string country)
        {
            switch (country?.Trim(' ')?.ToLower())
            {
                case "ca":
                case "can":
                    return CurrencySymbol.CAD;
                case "us":
                case "usa":
                    return CurrencySymbol.USD;
                default:
                    throw new Exception($"A currency for country with value <{country}> cannot be found");

            }
        }

        public static string GetCountry(string country)
        {
            switch (country?.Trim(' ')?.ToLower())
            {
                case "ca":
                case "can":
                    return "CA";
                case "us":
                case "usa":
                    return "US";
                default:
                    throw new Exception($"A country code cannot be detmined for <{country}>");
            }
        }

        // US and CA
        public static string GetStateAbreviationFromName(string state)
        {
            switch (state?.Trim(' ')?.ToUpper())
            {
                case "ALABAMA": return "AL";
                case "ALASKA": return "AK";
                case "AMERICAN SAMOA": return "AS";
                case "ARIZONA": return "AZ";
                case "ARKANSAS": return "AR";
                case "CALIFORNIA": return "CA";
                case "COLORADO": return "CO";
                case "CONNECTICUT": return "CT";
                case "DELAWARE": return "DE";
                case "DISTRICT OF COLUMBIA": return "DC";
                case "FEDERATED STATES OF MICRONESIA": return "FM";
                case "FLORIDA": return "FL";
                case "GEORGIA": return "GA";
                case "GUAM": return "GU";
                case "HAWAII": return "HI";
                case "IDAHO": return "ID";
                case "ILLINOIS": return "IL";
                case "INDIANA": return "IN";
                case "IOWA": return "IA";
                case "KANSAS": return "KS";
                case "KENTUCKY": return "KY";
                case "LOUISIANA": return "LA";
                case "MAINE": return "ME";
                case "MARSHALL ISLANDS": return "MH";
                case "MARYLAND": return "MD";
                case "MASSACHUSETTS": return "MA";
                case "MICHIGAN": return "MI";
                case "MINNESOTA": return "MN";
                case "MISSISSIPPI": return "MS";
                case "MISSOURI": return "MO";
                case "MONTANA": return "MT";
                case "NEBRASKA": return "NE";
                case "NEVADA": return "NV";
                case "NEW HAMPSHIRE": return "NH";
                case "NEW JERSEY": return "NJ";
                case "NEW MEXICO": return "NM";
                case "NEW YORK": return "NY";
                case "NORTH CAROLINA": return "NC";
                case "NORTH DAKOTA": return "ND";
                case "NORTHERN MARIANA ISLANDS": return "MP";
                case "OHIO": return "OH";
                case "OKLAHOMA": return "OK";
                case "OREGON": return "OR";
                case "PALAU": return "PW";
                case "PENNSYLVANIA": return "PA";
                case "PUERTO RICO": return "PR";
                case "RHODE ISLAND": return "RI";
                case "SOUTH CAROLINA": return "SC";
                case "SOUTH DAKOTA": return "SD";
                case "TENNESSEE": return "TN";
                case "TEXAS": return "TX";
                case "UTAH": return "UT";
                case "VERMONT": return "VT";
                case "VIRGIN ISLANDS": return "VI";
                case "VIRGINIA": return "VA";
                case "WASHINGTON": return "WA";
                case "WEST VIRGINIA": return "WV";
                case "WISCONSIN": return "WI";
                case "WYOMING": return "WY";
                case "ALBERTA": return "AB";
                case "BRITISH COLUMBIA": return "BC";
                case "MANITOBA": return "MB";
                case "NEW BRUNSWICK": return "NB";
                case "NEWFOUNDLAND AND LABRADOR": return "NL";
                case "NORTHWEST TERRITORIES": return "NT";
                case "NOVA SCOTIA": return "NS";
                case "NUNAVUT": return "NU";
                case "ONTARIO": return "ON";
                case "PRINCE EDWARD ISLAND": return "PE";
                case "QUEBEC": return "QC";
                case "SASKATCHEWAN": return "SK";
                case "YUKON": return "YT";
                default: 
                    throw new Exception($"A State code cannot be detmined for <{state}>");
            }
        }
    }
}
