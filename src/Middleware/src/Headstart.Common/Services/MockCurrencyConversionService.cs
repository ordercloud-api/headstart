using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Services
{
    public class MockCurrencyConversionService : ICurrencyConversionService
    {
        public Task<ConversionRates> Get(CurrencyCode currencyCode)
        {
            var mockRates = new ConversionRates()
            {
                BaseCode = currencyCode,
                Rates = new List<ConversionRate>
                {
                    new ConversionRate
                    {
                        Currency = CurrencyCode.AUD,
                        Symbol = "$",
                        Name = "Australian Dollar",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAPFude90fOtkbulsdupye99XZuVea+VfbOZ0f+eBi+qJk/TP1O69xOzCyeWvuuW3wezJ0enGz+3W3dalt8uZrsmWrLWSrJRumaOEqYl0o760znRfl5qOtoR5qWtinmxjnnlxpn15rQEBUgICVQcHaggIawkJawgIWwsLbQ0Nbg4Obw4ObgwMWRAQcBERcA8PYhIScBMTcg8PWxYWchISXRgYdBoadhcXaRwcdh8fcigoey4ugjExgjg4iD4+iz8/jEFBjU9PlVBQllJSl1xcnmNjomhopHJyq3FxqnFxqX19sKOjyKKix7a208nJ3m5vqf///////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAFEALAAAAAAQABAAAAeVgFGCg4SFhoeFFBwhACAbE0CRkkCCGQ0SChALGkidTkdIPoICCQQFAwgBQUKsQkU9gh0RDgcPDBg/ublDPIIWFx8GHk8VO8bGTTqCOTYozs9MzzY5gjczJtjZ2UQzN4IvNSRGJCsk5ucwL4InMUpQSSkl8vMtJ4IjLipLKvkqKjj7VIwQJEKEDBosCipciKihw4cPAwEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.BGN,
                        Symbol = "лв",
                        Name = "Bulgaria Lev",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAALATGLMUGd0dI90eJLobIN4jKd4lK98mLMIhJh+XWiGYXCucYimPWy+eZjKXZTufbEGnc0KndFCufv////z8/P///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAABUALAAAAAAQABAAAAVOYCWOZGme5aSuLCu28PrGsEjRLCU+UgT9vt9P8hA5GoqkctlwiBiLhHRKXTBEiINgy+0eECKCYUAumw0EUaDAbrvZARFgTq/XUfi8Xh8CADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.BRL,
                        Symbol = "R$",
                        Name = "Brazil Real",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAAkomT9UpQcnjwkqmQsskj9YpgcqkS5Oqg0xjA0zjBg9kDFYml6Eu19/sHaSu0p1qUpfc0xgcnGBjE5ga2WAiICNjA+QXVWwjFWxjApyRhqVXxuXYhuVXxyXYUqsgUqsgluzjHjBoXnCoRqVWRuVWXvCoBJ2QxJ1RBN2QxR5RR2WWCCZXSGHUyiPWiiPWzmebEOmc0qreU+tflGvf1Guf120iV60iXzDoCCXVxt/SyOaXCOZXCWZXU+tfFKvf1GtfXG9lX7Dn0SnS3nAf3zCf3O8dEurSVmlWFmvVHe9cmCxUmGoVoLBPrTajrbbjYnFPKDNZYfBN6XQZIKLcpHFNJaicXyHV7XPLMvdaLvQKs7eaLa9ftLeSM7ZJdfgRtXcIv/5Df/5DvXtJ/HrKf/6M+7oUv/zE//0E//xFuXbH97UHv/2MeriU/vqGMrAL8q/MN7Vad7Waf///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAHIALAAAAAAQABAAAAeqgHKCg4SFhoeFPoqKMx4fNIuKgj1BlSIlTU43IZVBPYIxQDUYQ1hxcFpEFzZAMYIwIDJQZRUFARJsUj8gMIIvRVxrWwwODQdTZF5JL4JHY2ZoEQQKCw8UbWdiS4IuSF1gVgAJCANVYV9KLYIsHSpRaRMCBhBqVDgbLII5PCQWQldu3mQxYmEEjxyCUuxYyGEFkyc6NCzckUIQiosXTWTIcALjRUQgQ4oUGQgAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.CAD,
                        Symbol = "$",
                        Name = "Canada Dollar",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAPKpqvXCw64HDbIJD64JDrILENsQF9sRGLkPFNsTGbkQFtwVG9wVHNwXHsEVGtwZH8EXHN0bId0cIt0dI90eJN0fJcgdI94hJ8gfJN4jKd4kKd4kKt4lK9AmLN4qMNAoLd8sMdgvNdgwNuE3POE4Pd44PeE6QN45P+I+ROJARuNCR+NDSeNFS+RHTeNHS+NHTORJTuRLUORPVOVWW+ZZXuZdYehpbet2eut3e++ZnPCfovSws/OvsfSztvOytPbExvrd3vnc3fre3/vj5PfNz/zs7P75+f77+//9/f/+/v///8zMzP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAEwALAAAAAAQABAAAAeVgEyCg4SFhoeFLTAwSo2OjYswgis3N4+PODcsgig2M41JQUFJjTM2KYIlNC+NRTU1Ro0vNCeCIS4kQwA7KjI8PUMjLiKCHRoLASAXGRwSMQEMGx+CFhUJSEQeFBMmQkcJFRiCDhEGjT45Oj+NBhEQgggPB45AQI4HDwqCAw0Nl479CggSQIDAkoMIDxYkgKihw4cPAwEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.CHF,
                        Symbol = "CHF",
                        Name = "Switzerland Franc",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAALIOFLUPFd8WHt8XH98YILwVG98bIt8dJOAeJsQbIuEgJ+AkLMskKuEpMNMtM9w1POQ7QuQ8Q+E+ROVFS+ZKUOZKUeZLUeZNU+hbYOhdYuttce16fu16f////////wAAACH5BAEAAB4ALAAAAAAQABAAAAVnoCeOZGme5aWuLCtSWyzP3EaJk4btWNfxGM1EJMlYjhYf0pKRiB6VCMRH7UAglofI0TAYqj5vwyFiLAhogi9NWDBECYVgLvDRBYqEqIAY+P+ACAUiAQeGh4iGASIAjY6PjyiSk5SUIQA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.CNY,
                        Symbol = "/元",
                        Name = "China Yuan Renminbi",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAAN4kJt4rL+NERuNFRuVTVeVUVuuAgbATF7MUGN0dIt0eI90fJN4gJrobH94hJt4jKN4lKt8mK8IhJuAuNMkpLeAvNNIyNto7P+NARuNBRuNCRuBDR+NKTuVPVuRPU+VQVeVRVeVRVuVSVudeYudeY+dfY+hiZupxdOx+get9get+gd0aJOI8R98/R+NGTuRJVfvsW+t+Qe2IUe2JUu+SX/KnfvGece2NgOhkU+pzZOVVVf///wAAAAAAAAAAAAAAACH5BAEAADsALAAAAAAQABAAAAaDwJ1wSCwaj0WRqBMqzEIhpVQk9NxqN4OqZkipVCiVR+iywWil0mhESpdOHGErJ8O9QDLdB8Q3bYQXBCwZAzEaAhgZih8XQhYTDAwADg4MlJQVFkIUAQueK56hCwEUQhIRCamqqxESQg0QCrKztBANQggPuru8ughCB8HCw8NIxsfIyEEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.CZK,
                        Symbol = "Kč",
                        Name = "Czech Republic Koruna",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAALUSFtwYH7ATGK8TGNsYH7MUGdwdI9weJNwfJbobIN0hJ90jKd0kK90lK94mLMIhJsgkKd8vNdEyN9EkLsAgMawhOZclR3UlVEQcTUcvci4tehwugT5Tnj9UnvDy+S5HlxIxiFJpqmN3sgsuixA3lCBAjClJlTlYoj9co4SWwwIkdAcwjgkxjgoxigszjgkpdAwyixAxfBM4jhY8kBc4hBs/ki9QnDBRnDJSmzlYoT9epFNurWyDuYGVxKa01Nrg7eLn8ay/3vv8/f////z8/Pb29v///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAEYALAAAAAAQABAAAAd7gEaCg4SFhoeFPR5DjI2OjIIcKT+PlYI5IiE+QpWNgjg7KB08QJ1EgiY6NzYfJ0GVRYIlNTAwLiMXCrq6ERKCNDMtLCAWCAEEyAQMEIIxMisbFQbT1AYOD4IvJBoUB97f3g0JgioZEwvo6eoFghgCAAPx8vMDiPb3+PiBADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.DKK,
                        Symbol = "kr",
                        Name = "Denmark Krone",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAALATGLMUGd0dI90eJLobIN4jKd4lK98mLMIhJuBDR+BDSONKT+RPVOVRVeVSV+deY+dfY+hiZupxdOx+gut+gf////b29u/v7+fn58zMzP///wAAAAAAAAAAAAAAAAAAACH5BAEAABoALAAAAAAQABAAAAVcoCaOZGmepaM6Vbu+IkPNbTXPE8WIiwRBtcfvJ1mIFJFGo6ZsRhIiS21KtYgu1GzlIsJoqRgR4iAQ1MroA0JEMAwGtbfcQBAFCvgafl8IiACAABmDgYUoh4iJiSEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.EUR,
                        Symbol = "€",
                        Name = "Euro Member Countries",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAADzSURBVHgB3VJNCwFRFD3jMxOlZI98LUWpWRApf4MdP8CPYecPKGsbRZqFyNI3axJFFMZ481b0niEb5SxevfvePfeecy/wawj0jFVUfINeQTDpvfvdK1xvRuxONuyOIvePgRf0uDbISzJmazdNTgXHcIrHzwji3gWpakBVlqAShVrl+iBCtUq+OUPASOguPeBhS4jkue99BznSuqoKXJJStgGr6fIUY6agaX1lGAMyBaaDTHjINUwztphsMXGGoNaPwmxUkA6N6F20nJEITKAQY8utJEPA3YP13oHmKEQTNwc7OlM/ifJ3TXeR2pPAw03An+IOKBNIxV4yaAsAAAAASUVORK5CYII=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.GBP,
                        Symbol = "£",
                        Name = "United Kingdom Pound",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAP2gov2hovuvsvuwsv64usBYX/F1ffKNlMmbnvvGyfzd3/rs7dMNItINIdMPI9IPI9IQJNIQJdMRJdIRJdMSJ9QTKNMTJ9QUKNMUJ9QZLNUgM8ksPMktPdkzRdk0Rtk1Rtk2R9I2RtM4R91FVdxEVd1HVt1JV91KWuBTYuBXZrhLV6pGUKtJU+Z1geZ4g8Sxs/Th4/Xe4fXf4vno6t9/jd7Kzfjl6NRdcfjn6shedsFXct2drPbm6sOvtdCQpMiInotBXLBVd8mcs6VxlYxJefPu8+fk7d/d6jw1cRUUaElIkGJhoAEBSQ4OZA4OYw8PYQ8PXSAheDg4hjg4gkFBiElKkSUofzo8ihMZdSsvgnZ5sX6Bs4GDtDA1d0RKlYGFt+Tl8HF3r9TW52lym4iUxqKr0bK72s3T58fP5v+5t/+QkP/JyP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAGwALAAAAAAQABAAAAevgGyCg4SFhoeFNGtlVVQ2JyU2Sl5DBzyCSzsKZ1wLLi0LX0IERWGCUlo/AkY4KSg4PgNgW1OCMDM2uTYmI7o2MzCCISQfHh4dIMUgxiMighwaExIRDxYWDxASExobgjUyMeExFw7iMTI1glBZaAA5MRQNMWIBOldPgl1HakFWMRUMYkQxk+YGEkE9DBDB4iRGBgwxkjQhk0CFoAJAmGh8wWLFC41MxiBARLKkSZOBAAA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.HKD,
                        Symbol = "$",
                        Name = "Hong Kong Dollar",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAOZbXut+gPOvsPfHyNsSGdwTGdsWG9wYHrATGLMUGd0bIdwbId0cIt0dI94eJN0eI90eJN4fJd0fJbobIN4hJt4hJ94jKd4kKt4lK98mLMIhJt4rMN8tMskpLuAvNeAxNuAyONIyN+E3O+E3POI7QOI8QeI9Qto7QOBDR+BDSONKT+RNUeVOU+RPVOVQVeVRVeVRVuVSV+VUWOVXW+dbX+ZaX+ZcYOdeYudeY+dfY+ZgZOdhZehiZudkaOhna+hscOpxdOp0eOt4fOt8gOx+gex+gut+ge+Qk/Ccn/GlqPKqrfKrrfOztfW9v/a/wfjV1vnd3vvj5POztvrd3/zr7Pzq6vzs7P3w8P3x8f///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAFkALAAAAAAQABAAAAehgFmCg4SFhoeFMYqLMTCMioItRpOTQkOURUYtgipAOTg3AEgCMzY5OUAqgik8LjIsNVgDOzcrLzwogicuJjRPSk5LVVY+JC4ngiEeERdNVD1BTEQgDh4hgh0bEgsSR1JJUyMGEhsdghoZDQciUVABVzQEDRkaghMYEAofPyUFOhwMIGCYICiBhYMUDjywwKDCQQsJBCGYSLFiRUQYM2rUGAgAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.HRK,
                        Symbol = "kn",
                        Name = "Croatia Kuna",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAAOaHidoQF9oTGdsVG+A3PN45P+JARuFARuJFS+JHTONJTutOUulPU+ROVOhQVOVQVehSWOVRVeVRV+VWW+RWW+VZXuhpbet8gOx+gut9gdyCidyFjdKIj7qSltSJlsuVnsWIlY+FlW9WgG5Wfw4wjA4xjA80jA8yiw8ziwIibAMjbggviQkviQkpdBE2jRAxfBM4joWUrYiZpICinuvCwuvDw+7Ly/////b29u/v7+fn5////wAAAAAAAAAAAAAAACH5BAEAADsALAAAAAAQABAAAAaJwJ1wSCwaj0WFZBlxMBaMx3IpRGQyF4Dn8/EALphMQ2iwUCidWGgm60AmloOwUEkkQJyNhgOyVwpCODeDNgQ3hjaDNzhCOYo1NwORkIM5QjqKNAI3mzSKOkIvMCukIwE3I6QrMC9CLS4ssSwiIrIsLi1CKia8JigkJSe9JipCKcfIyclIzM3OzkEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.HUF,
                        Symbol = "ft",
                        Name = "Hungary Forint",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAAN8xO98xPOM4QuQ+R+Q/SeVBS+ZPV+ZQWOdSW+ljauxyeetxeAFiKgJkLAd9OQh9OghpMg6APxCBQA9yORKDQv////b29u/v7+fn5////wAAAAAAAAAAAAAAAAAAAAAAACH5BAEAABkALAAAAAAQABAAAAVVYCaOZGmeZaGuLCsOSyzPyjKIQnLsh2Hwh4RAFEAQjsgkAiCyVJ7Q6NMiukivlYsIg5ViRBOKY0wuUyYiiOTBbrslEFEjQq/b6Q0RY8/v91GAgYKCIQA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.IDR,
                        Symbol = "Rp",
                        Name = "Indonesia Rupiah",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAANo7QONBRuNCR+BDR+BDSONKT+RPVOVQVeVRVeVSV+deY+dfY+hiZupxdOx+gut+gf///+/v7+fn59/f39fX19DQ0MzMzP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAABcALAAAAAAQABAAAAVQ4CWOZGmeZaKuLCsaTyzPzmOIRbPsi6Lwi0ZBRGAgjsgkYyACHASBKDQaPQBEEYh2y9VGRJKuGCIRTcbdiYiC5lJElfa2IrLY73g8as/v90MAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.ILS,
                        Symbol = "₪",
                        Name = "Israel Shekel",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAIcAAAA70QdB0ipb2C9e2Tdl2zpn2z1p3EFs3GaI42iK42mL42uN5G+Q5XCQ5XOT5XWU5r/M77DC8LPE8bjI8r/O88jT8M/a9tLc99fg99jg8+Dn+ePp+ebr+ufs+u3w5PP1/Pj5/f///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAMAAB4ALAAAAAAQABAAAAhrAD0IHEiwoMGDBQEoXMiQocCGEBcK7ECxooULFSsKDMExhAYLCBRY2NAxxMaOEQw4aECAQsmTHQWA+DCgpEkPJSUUeLDgwISXODtywJCAAYYOQDMoXQqhwtKlDyNCFBigqtWrVxFq3cqVa0AAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.INR,
                        Symbol = "₹",
                        Name = "India Rupee",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAALbA3IiZxoqcx7bB3Ku41snQ4Ozx+unv+RN2RBR5Rh2WWR6WWRt/TCOZXSWZXiGHUyaaYOuVS++aUvCcV/CdWPCeWvGlZvGmZvGnaPOwd/S2g/S3hP////b29u/v7+fn5////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAACAALAAAAAAQABAAAAZoQJBwSCwaj8WKcslkCieaqHS60UyEksxle7FYuJeMRBjBUM7oNCYi7HDeHANgYIBzOkKPvSAIFOweQh92BwQEB3YfQg8QCo6PkBAPQgwOC5eYmQ4MQgkNn6ChnwlCCKanqKhIq6ytrUEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.ISK,
                        Symbol = "kr",
                        Name = "Iceland Krona",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAP8IC/RDR/NGS95SYdxRYNxRYdVPXd5Zad1Zabl0qbR3sbmEtqZkqKJipKJgp7OIvp9msJ9mr5tkq5phrwAAzAAAygAAyQAAyAAAxwAAxQAAxAAAwSsu2Swv2DM22jM22DE00Qkb3Qwe3wwd2hAh3Ase4A0f3g8i4RAi4S1B6S5B6TNH6zFE4jNG6TVK7TdM7T1k/jlh/f9TKf9QKflOKP9DJP9KK/9sX/9tX/1rXvVoW/9zZf90Zv9MR/8QDf9HRP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAEAALAAAAAAQABAAAAeNgECCg4SFhoeFFRUYLQQ5H4qRioIVFBYrAzceFJydFYIZiyoFNx2SihmCIyIhMQg7LiUis7MjghIREw8CPQoQEcDAEoI0MzI2PgA1M8zNNIINDA4LAT8JDNjZDYIkKCYwBzwvJyjl5SSgiykFOBynFalAF5aYmp2dF4IbGhssBjogNAgcuAGRwYMIEQYCADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.JPY,
                        Symbol = "¥",
                        Name = "Japan Yen",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAANoHCdoHCtsJDdsLDeEuMuI/QOI/QfOoqfS6u9wNE+IzOO+RlfSws/fGyPnV1twJE9wJFNwJFt0LGOIuN+IuOOIuOeIvOuM6ROM9RuhZYOppcex4f+x6ge2Eie6GjO+QlvW3uvjQ0vrc3v309dkHB9sLC+E/P/75+f78/P////z8/Pb29u/v7+fn59/f39fX19DQ0MzMzP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAADIALAAAAAAQABAAAAZrQJlwSCwaj8WUcslkCpvQ5XOJAnE2jJNTplouTAWDqbNUCVdKkWJiqVAIIeVKyFIiBpK8pHRQsoQtSg4JDxEREAINSi1CLksaJAABJBlLLkIvSycfGBceI0svQjBRTTBCMamqq6tIrq+wsEEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.KRW,
                        Symbol = "₩",
                        Name = "Korea (South) Won",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAPGZms4jK9dDSPnY2bs1Rv/7/KVadoFQejAZZDMweSsuewkfgGx8tAclhAcjgH2NvgssiTdSnL3G3ujs98HK4fb4/QkyjDtfp/L2/vv9//b///v//7m8vPz//9DT0/Dx8err6///+8rKx////evr6q6tq//z8uYXFOMxMepCP+lJSOVJSfJ1cux7e++Li/na2v/5+f/+/v////7+/v39/fz8/Pv7+/j4+Pb29vX19e/v7+fn5+Xl5eTk5OPj49/f39nZ2dfX19DQ0M3NzczMzMvLy8rKysnJycjIyMfHx8XFxcPDw8LCwr6+vru7u7q6uq+vr6SkpJ2dnZaWlpGRkY+Pj42NjYSEhH5+fnh4eHFxcWlpaVxcXFlZWVdXV1ZWVv///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAGAALAAAAAAQABAAAAemgGCCg4SFhoeFMjI1NIqOMjQ1ioKKSkc2jzZDTJNgMkBYVyAxLwMFH1VWQTKCNUVZHDAtKyosJh5ZSzWCODI8GgAoKQIELhs9MziCOoodBicBCRAHIYo6gjsyPSMPCAoWCwwjPjI7gj9PXiUYFw0OERMiX1A/gkFNXF0+GRQSFSRbtDgJIkiIjClScjy6EYWKDCGCiBBBYkSiRSJGkkhExLGjR4+BAAA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.MXN,
                        Symbol = "$",
                        Name = "Mexico Peso",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAK8SF7MUGNwcItwdI9weJLoaH90gJt0iKN0jKN0kKt4lK8IgJt4qL8koLd8uNNExNtk6P+JARt9CR+NJTuROU+RPVeRQVeVRVuZeY+dhZulwdOx+get9gfL69xJ1QxR4RRyVWB2VWB6WWRp+SyCXWyKYXCOYXCSYXSWZXyCGUyqbYSiOWi6eZTGWZDqea0CmckGnc0Klc0mqeU+tfVGuf160iGG1inC8lX3Cn06tfFCtffX58/H27cPLu4SOebfEp8rOxcbNurfBpYeFX7qsioppLIphLqF8UaeIZOXWyPnx7fz5+f///8zMzP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAE4ALAAAAAAQABAAAAeQgE6Cg4SFhoeFNIqKTI1MF5CQgjk4lTiOTBwcGxwUgjI3NaKYGKUaE4IxNjqsTEtKTBayGRKCLjMwLy8dSEZJEcAVEIItLCTHQEdFRAbNDg+CKyoi1D8+Q0IE2gwNgikoIOE8QT07AucKC4IjJyHumAPxCQWCHyUm+JgHCPwBgh4AATYZ2ASAQYOIEipcuDAQADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.MYR,
                        Symbol = "RM",
                        Name = "Malaysia Ringgit",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAALoBBugIDugID+kJEMQIDcQIDukQF+oWHNQXHOQoLuwuNO04Pe0+RO5BR+9PU/BQVO9QVPJjZz9QrVBetQsskQsrkBs3jh87kU5jpEFcqEFdp0FcnHyOvDhYmHiNuF10lk9qjjhXc1VviFVrgGV8knCGm2+FmoudrURifGJ7kZSirHqMilVxZpSfjqSul6uyYKuyacXGZNTUjdfVWdvZZ9vZaP////z8/O/v79/f39DQ0P///wAAAAAAAAAAAAAAACH5BAEAADsALAAAAAAQABAAAAZ1wJ1wSCwaj8UMaUXSaDCNqLQh/MhUntOJY+t6bUKYa1Ki1UyOB+TxiCyEr5YkNZulvt2bcBQDdUQiGwqDgwwJQhcoLCEUFRZ4NjhCCAcDlpeYBwhCOZCQOUIEBgIBAqakpQIGBUI6nng6QgCztLW1SLi5urpBADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.NOK,
                        Symbol = "kr",
                        Name = "Norway Krone",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAALIOFLUPFd8WHt8XH7wVG98dJOAeJsQbIuEgJ+E+ROVFS+ZKUOZLUeZNU+haX+hbYOhdYuttce16fu16fwEiagcvhwgvhwkxiAsyiQ40iho/kB9Aii5Qmj9eokFgpFBsqnGIuv////b29ufn58zMzP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAACUALAAAAAAQABAAAAZ5wJJwSCwaj8WGshHyhJZQ4UJCDYFCVOpEshAqIo9H6BNyhMMRhTABYTBCnZB7DkkIRaF8iKPvi4QbGhiDhIUaG0IjfRd9eiNCBwgCAiEVIZOYCAdCBAYDAyEWIZ+kBgRCAQWqIRkhqq8FAUIAtAAkFCS1uki8vb6+QQA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.NZD,
                        Symbol = "$",
                        Name = "New Zealand Dollar",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAPFude90fOtkbulsdupye99XZuVea+VfbOZ0f+eBi+qJk/TP1O69xOzCyeWvuua2wOW3wenGz+3W3dalt8uZrsmWrJ9mgruSp6BohbSIn7WSrJRumaOEqZ5+pY5vnYl0o760znRfl5qOtoR5qWxjnn15rQ8Obw8ObhAPcBIRcBYVZx4ddwcHVgkJbAoKagkJWQ4OcA4PcA4Pbw8QcA8PbxAQcBAQbxERcBIScQ8PXxMTchMTcRUWcxYWcxgYdRkadRYWZx0ddx0dbiIieiYmdyYmdi8vfjMzhTU1hjg4hjs8i0RFkUNDjkVGkUVFkEdHkVRUmVVVmVdXm2Njo3R0rHV1rHZ2rAMGVwQGVgsNbgwPchATcxgbbR8hey4xhENFkW5vqQsQchEWdf///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAGMALAAAAAAQABAAAAeegGOCg4SFhoeFFCIlAB0hE0+RS02Rgh8NEgoPCyBVnlVWVEyCAgkEBQMIAVBQURdRU0qCIxEOBxAMHE5fTr1SSYIaGyQGHmAVSMlHXhlGgkRDO9LTGDw4Ol1FglwrNi4tMSk3WTflQUKCKj8yNOztNCdaJj9Agjk+NTM1+ygwYRY1xOQQ9KKHwYMIe2x5IYiFw4cQr2BxiKiixYsXAwEAOw==",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.PHP,
                        Symbol = "₱",
                        Name = "Phillipines Peso",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAPnGx/avsrgPHecUJrgQHuYXKLkUIrwVI+geL+ceLugfL+cfL+ggMecgMMMbKegiMugjM+cjM+gkNOgmNuknOMwiMekqO+ksPNMqOOkwP9szQPrR1fe3wDg4hj4+jD8/jENDjkREj0VFkEdHkVBQllRUmVVVmVdXm2hopHJyq3V1rHZ2rNPT5rO0zy8ziMbH29bX6Cgugi40ij5Ek8zMpv//2///6///7v//9efn4vz8+t/f3f///v/93P/0Y//1bf/2e//3hP/80v/81O/syP/82Pbz0P/939KxsNi8u////9fX19DQ0P///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAE0ALAAAAAAQABAAAAeOgE2Cg4SFhoeFNS0hI42OjyOCSkovKSqXmCorKiCCPJMwJCYlJSamJigegjpHPUosHyKysycdgkZBQEU3SS4yv78zMYJEPz5CNkgPy8wZGoI5Q0I4GwzW1wwXGII7kwAWAwoI4+MUFYJLShwRBQ0L7/ATDoJMARAJEvn6+geCNAIEDAgcSNAAooMIEyYMBAA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.PLN,
                        Symbol = "zł",
                        Name = "Poland Zloty",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAALIOFLUPFd8WHt8XH98YILwVG98bIt8dJOAeJsQbIuEgJ+AkLMskKuEpMNMtM/////z8/Pb29v///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAABIALAAAAAAQABAAAAVJoCSOZGme5aOuLCu28PrGsAjRLCRG+BqJjoZhSCw2HCLGgsBsOhcMUUIhqFqvioSogBh4v2BEQRQ4mM9oc0AEaLvfb5R8TqeHAAA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.RON,
                        Symbol = "lei",
                        Name = "Romanian Leu",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAALIOFLUPFd8WHt8XH98YILwVG98bIt8dJOAeJsQbIuEgJ+AkLMskKuEpMNMtM9w1POQ7QuQ8QuE+ROVFS+ZKUOZLUeZNU+hbYOhdYuttce16fu16fwEiagIjbAcvhwgvhwkxiAgpcgsyiQ40ihA2iw8xexI4jBY8jhc4gho/kB9AiihJky5Qmi9RmzFSmjhYnz5doj5eoj9eokFgpFBsqlJuq2N8s3GIusy/B//vDv/vD//vEP/wEf/wE//wFv/yNf/yNv/zRf/zR//0VP/0Vf/2df///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAEYALAAAAAAQABAAAAeWgEaCg4SFhoeFM4qKQo1CFpCQgjA3lTdFmEUaGhsaFIIvNjSjRENDRBepGROCLjUysEGyQRW1GBKCKzEtLCw/QD8/ERAQFQ+CKikiyz3NPQbQDQ6CKCcg1zzZPATcCwyCJSYe4zrlOQLoCgmCISQf7zvxOgP0CAWCHSP6Iz79PgcADwQQxKFgQRwIcQBYuBCRw4cQIQYCADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.RUB,
                        Symbol = "₽",
                        Name = "Russia Ruble",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAALIOFLUPFd8WHt8XH7wVG98dJOAeJsQbIuEgJxFYqBNaqR1grR1ZnSJkriZjpS9qrTVytjZytkV9vP////z8/P///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAABUALAAAAAAQABAAAAVOYCWOZGme5aSuLCu28PrGsEjRLCU+UgT9vt9P8hA5GoqkctlwiBiLhHRKXTBEB4Rgy+0iDiKCYUAumw0EUaDAbrvZARFgTq/XUfi8Xh8CADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.SEK,
                        Symbol = "kr",
                        Name = "Sweden Krona",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAAAdChAlFhw5Xpw9XqA9XpxBXqA9KjhZbqhhcqxleqxZSlThysz54uUN8ukV9u0d+vFSHwVWIwVeKwmiVyHWezXafzf/xHfbqL8y/B//vDv/vD//vEP/wEf/wE//wFufaHf/xIu/iJv/yNf/yNv/zRf/zR//0VP/0Vf/2df///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAACkALAAAAAAQABAAAAZ4wJRwSCwaj8WH8lFqLp/CBmWKqk6nFUpDyJhEIqeTCfL9ThjChcThILnZcMlCeCGNRPg7Hk+6CEMgHYKDhCAhQh8WHIuMjRYfQgoJAgQZGhqUBJoJCkIGCAUDG6OhpQgGQgEHqx6tq68HAUIAtAAYt7W5SLu8vb1BADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.SGD,
                        Symbol = "$",
                        Name = "Singapore Dollar",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAAOM7POtvcPrc3ds1NeM5OeM7O+Q8PNw8POFEROZKSuVKSuVLS+VNTeZQUOZRUeVRUeZTU+daWuhdXehfX+hgYOhiYupra+txcedycux2dux4eOx8fOx9fe1+fux+fu1/f++IiO6IiO6Jie+MjO+NjfGenvGhofS5ufa8vPfIyP///+/v7+fn59/f39fX19DQ0MzMzP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAADEALAAAAAAQABAAAAZpwJhwSCwaj0VIRySJPCDQKFS4SZ1IpVFnu/10GkKNgBKqgCgTippyWQgxKFMgY2E47vcKQnjgeBIEAAYFhIQKA0IrKouMjYsrQiyOkyosQi2Uji1CLpmNLkIvnowvQjCnqKmpSKytrq5BADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.THB,
                        Symbol = "฿",
                        Name = "Thailand Baht",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAMQAALIOFLUPFd8dJOZKUOZNU+16fu16fwkxiAsyiRY8jhc4gho/kB9AiihJky5Qmi9Rmz5eov////z8/N/f39fX1////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAABUALAAAAAAQABAAAAVPYCWOZGmeJaGuLCsORSzPRjGIUa7vuyjxwIhE1IA8HMgjEglpiBgLhHRKXTBEisRhy+0mFKJJkDcRUcY7iiggaLvf7YAIQK/b7ai8fr8PAQA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.TRY,
                        Symbol = "₺",
                        Name = "Turkey Lira",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAOM7PORHSPCPkO+Oj/CQkf3x8v3y89wKCrENDd4XF98ZGLIUFN4aGrUVFd8cHN4cHN8dHd4dHd8eHt8fH94fH98gILwbG98iIt8kJOAlJeAmJuEnJ+AnJ8QiIuAsLMsqKuIvL+EwMOIzM9MzM+M3N9w8PORAQORBQeRCQuRFReFEROVGRuVLS+VMTOVNTeZQUOZRUeZTU+hWVudaWuheXuhfX+lgYOhgYOhiYulkZOpra+psbOpvb+txcepxcet4eOx5eex6eux8fO1+fu1/f+6Fhe6IiO6MjPCTk/GhofStrf319f/+/v///////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAE4ALAAAAAAQABAAAAeggE6Cg4SFhoeFMS4xjI2OjIIvQUNBP0JDmJhEQy+CLDszQEhANDU3pzc9LIIqMklLSkY5LS0wtjgqgiU6TUYgACZFKCsnKDAlgiM+TCIPFQQGBBwTFyEjgh8uTTsHCQ8CFRkTFR4fgh0KRwUDNiQJEhASEhsdghYOCQE8KfIT/hMaLAhqEAFDBAYUMChciKGBoAUIFkicSFEioosYM2YMBAA7",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.USD,
                        Symbol = "$",
                        Name = "United States Dollar",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQANUAALoBFOkIH+oJIMUIG+oQJusWLNUXKuQoPO0uQu04S+4+Ue9BU/BQYPJjcQ4ObRAQbxsbax8feCwsfTMzhDw8iUNDjUVFj1JTl1JSllNTl1VVmHx8t3R0q319t4ODu4CAsZCQwo+PwJmZx5iYxpqax6Ghy6ur0bOz1f////z8/O/v79/f39DQ0P///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAC0ALAAAAAAQABAAAAZswJZwSCwaj0ULSckkLZ5Q4YhzolpR2KyQYrqUMiVMiUFmNBLCkEZUYbOz2JRQIpqA7HaEHqE4CD8RHQ+CgnAoKkIQHg4bjIwCkAIFBkIrhpcoK0IDBAGen6AEA0IsmIYsQgCqq6ysSK+wsbFBADs=",
                    },
                    new ConversionRate
                    {
                        Currency = CurrencyCode.ZAR,
                        Symbol = "R",
                        Name = "South Africa Rand",
                        Rate = 1.0,
                        Icon = "data:image/jpg;base64,R0lGODlhEAAQAOYAAOY+Q+ZBR+VBRehPVO1xde1ydul0d+1+gu+LjhIRehYVexQTaB0cfxARYRQUYxUVZRwcfR8ffyQkgmRmo11fkmttp4GJrmZsiYWMsCxcNzt2SUF6TmmWdHSefjx3SWiVco+xlgdBEAg/ED9vRg5EFUFwR1B3VXGQdQEzBkFxRQQ2Bgc+Bwg+CAlACQtCCw5EDhZJFhpOGhdFFx9OHyhWKC5dLi9eLz5qPj9pP1B2UHGRcVlzCm+PEHCQEnGRFHWTGYahNo6oRJCpRqS5aIuHDnVyDIyIEI+LF52aM6GePK6qVSAbDzk2MdybmN+in+W0suY4OOlPT+lQUOtjY+xxcRcWFj4+PigoKB8fHxoaGhcXFwsLC////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAEAAFwALAAAAAAQABAAAAeYgFyCg4SFhoeFJSkcTQECAZCRkIJBJzogTwRUBJwEBQQAgklDJjkdTgNRUqtSU1CCTEpCIzgfB7e4CAaCV1ZIQDU1NsHENzSCWFlbRj4uzs8uMTOCWlVEPS3Z2tkwMoJLRzwhKxoT5ucVFIJFPyIsHhYRCQr0CgwLgjskLxsYEhASAgp8IAiFigwXHDRwwLAhQ0QQI0qUGAgAOw==",
                    },
                },
            };
            return Task.FromResult(mockRates);
        }
    }
}
