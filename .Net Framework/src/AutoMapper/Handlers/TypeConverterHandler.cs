﻿using JsonToJsonMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonToJsonMapper
{
    class TypeConverterHandler : ITransformationHandler
    {
        /// <summary>
        /// Converts value from one type to another
        /// </summary>
        /// <param name="config"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public dynamic Run(JObject config, JObject input)
        {

            object value = input["value"].Value<object>();
            string dataType = config["DataType"].Value<string>();
            string format = config["Format"].Value<string>();

            if (value==null || ((JToken)value).Type == JTokenType.Null)
                return value;
            try
            {
                if (!string.IsNullOrWhiteSpace(dataType))
                {
                    switch (dataType.ToUpperInvariant())
                    {
                        case "LONG":
                            return Convert.ToInt64(value);
                        case "JOBJECT":
                            {
                                JToken Jtokenvalue = (JToken)value;
                                if (Jtokenvalue.Type == JTokenType.Object)
                                    return (JObject)Jtokenvalue;
                                else
                                    return JsonConvert.DeserializeObject(Jtokenvalue.ToString());
                            }
                        case "JARRAY":
                            {
                                JToken Jtokenvalue = (JToken)value;
                                if (Jtokenvalue.Type == JTokenType.Array)
                                {
                                    if (Jtokenvalue.Any())
                                        return (JArray)Jtokenvalue;
                                    else
                                        return null;
                                }
                                else if (Jtokenvalue.Type == JTokenType.Object)
                                {
                                    return new JArray(Jtokenvalue);
                                }
                                else
                                {
                                    if (Jtokenvalue.ToString().StartsWith("[") && Jtokenvalue.ToString().EndsWith("]"))
                                    {
                                        return JArray.Parse(Jtokenvalue.ToString());
                                    }
                                    else
                                        return new JArray(Jtokenvalue);
                                }
                            }
                        case "SHORT":
                            return Convert.ToInt16(value);
                        case "INT":
                        case "INTEGER":
                            return Convert.ToInt32(value);
                        case "GUID":
                            return new Guid(value.ToString());
                        case "DATETIME":
                            {
                                string valueType = value.ToString();
                                Regex x = new Regex(@".*[+-][0-9][0-9][:]");
                                if (x.IsMatch(valueType))
                                    return DateTimeOffset.Parse(valueType, CultureInfo.InvariantCulture);
                                else
                                    return DateTime.Parse(valueType, CultureInfo.InvariantCulture);
                            }
                        case "CUSTOMDATETIME":
                            {
                                string valueType = value.ToString();
                                Regex x = new Regex(@".[+-][0-9]{4}");
                                if (x.IsMatch(valueType))
                                {
                                    int pos = valueType.IndexOf("Z", StringComparison.OrdinalIgnoreCase);
                                    String datetime = valueType.Substring(0, pos);
                                    return datetime;
                                }
                                else
                                {
                                    return null;
                                }

                            }
                        case "BOOL":
                        case "BOOLEAN":
                            return Convert.ToBoolean(value);
                        case "DECIMAL":
                            return Convert.ToDecimal(value);
                        case "DECIMAL?":
                            {
                                decimal decimalValue;
                                if (decimal.TryParse(value.ToString(), out decimalValue))
                                    return decimalValue;
                                return null;
                            }
                        case "INT?":
                            { 
                                    decimal dval = Convert.ToDecimal(value.ToString());
                                    return Decimal.ToInt32(dval);
                            }
                        case "GUID?":
                            {
                                Guid guid;
                                if (Guid.TryParse(value.ToString(), out guid))
                                    return guid;
                                return null;
                            }
                        case "DATETIME?":
                            {
                                DateTime datetime;
                                if (DateTime.TryParse(value.ToString(), out datetime))
                                    return datetime;
                                return null;
                            }
                        case "UTCDATETIME":
                            {
                                DateTime dateTime;
                                if (DateTime.TryParse(value.ToString(), out dateTime))
                                {
                                    string utcDateTime = dateTime.ToString("s");
                                    var zone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
                                    utcDateTime = utcDateTime + "+" + zone.BaseUtcOffset.ToString();
                                    return utcDateTime;
                                }
                                return null;
                            }
                        case "STRINGTOUTCDATEFORMAT":
                            {
                                //This block converts the given date into UTC formatted string, it will not change the TimeZone offset
                                //compared to previous block.
                                DateTime dateTime;
                                if (DateTime.TryParse(value.ToString(), out dateTime))
                                {
                                    string timeInUTCFormat = dateTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
                                    return timeInUTCFormat;

                                }
                                return null;
                            }
                        case "REMOVEDATETIMEOFFSET":
                            {
                                string valueType = value.ToString();
                                if (valueType.Contains("Z"))
                                {
                                    int pos = valueType.IndexOf("Z", StringComparison.OrdinalIgnoreCase);
                                    string datetime = valueType.Substring(0, pos+1); //including 'Z' in the output
                                    return datetime;
                                }

                                return valueType;
                            }
                        case "FORMATTEDDATETIME":
                            {
                                DateTime dateTime;
                                if (DateTime.TryParse(value.ToString(), out dateTime))
                                {
                                    string timeInUTCFormat;
                                    if (!string.IsNullOrWhiteSpace(format))
                                        timeInUTCFormat = dateTime.ToString(format);
                                    else
                                        timeInUTCFormat = dateTime.ToString(null, CultureInfo.InvariantCulture);
                                    return timeInUTCFormat;

                                }
                                return null;
                            }
                        case "STRING":
                            {
                                if (value.GetType().Equals("JValue"))
                                    return ((JValue)value).Value<string>();
                                else
                                    return value.ToString();
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed while trying to cast value into " + dataType + ". " + ex.ToString());
            }
            return value;
        }
    }
}

