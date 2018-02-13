public static class GraphQlObjectParser
    {
        public static string Parse(string queryType, string queryName, GraphQlSubSelection[] subSelections, object @object = null, string objectTypeName = null)
        {
            var query = queryType + "{" + queryName;

            if (@object != null)
            {
                query += "(";

                var queryData = string.Empty;
                if (!(@object is String) && @object is IEnumerable)
                {
                    queryData += objectTypeName + ":[";
                    var isFirst = true;
                    foreach (var obj in (IEnumerable) @object)
                    {
                        queryData += (isFirst ? "" : ",") + "{" + BuildQueryData(obj) + "}";
                        isFirst = false;
                    }

                    queryData += "]";
                }
                else if (@object is String)
                {
                    queryData += objectTypeName + ":\"" + @object + "\"";
                }
                else
                {
                    if (objectTypeName != null)
                    {
                        queryData += objectTypeName + ":" + "{";
                    }

                    queryData += BuildQueryData(@object);

                    if (objectTypeName != null)
                    {
                        queryData += "}";
                    }
                }
                
                query += queryData + ")";
            }

            if (subSelections.Length > 0)
            {
                query += "{" + BuildSubSelection(subSelections) + "}";
            }

            query += "}";

            return query;
        }

        private static string BuildSubSelection(GraphQlSubSelection[] subSelections)
        {
            var subSelection = "";
            var isFirst = true;
            foreach (var graphQlSubSelection in subSelections)
            {
                subSelection += (isFirst ? "" : ",") + graphQlSubSelection.Name;

                if (graphQlSubSelection.Params != null)
                {
                    var subSelectionParams = "(";
                    foreach (var param in graphQlSubSelection.Params)
                    {
                        subSelectionParams += (subSelectionParams.Length > 1 ? "," : "") + param.Key + ":\"" + param.Value + "\"";
                    }
                    subSelection += subSelectionParams + ")";
                }

                if (graphQlSubSelection.SubSelections != null && graphQlSubSelection.SubSelections.Length > 0)
                {
                    subSelection += "{" + BuildSubSelection(graphQlSubSelection.SubSelections) + "}";
                }
                isFirst = false;
            }

            return subSelection;
        }

        private static string BuildQueryData(object @object)
        {
            var queryData = string.Empty;
            foreach (var propertyInfo in @object.GetType().GetProperties())
            {
                var value = propertyInfo.GetValue(@object);
                if (value != null)
                {
                    var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    if (type.IsArray)
                    {
                        var values = value as ICollection;
                        if (values.Count > 0)
                        {
                            queryData += "," + char.ToLowerInvariant(propertyInfo.Name[0]) +
                                         propertyInfo.Name.Substring(1) + ":[";

                            var queryPart = string.Empty;
                            foreach (var v in values)
                            {
                                queryPart += queryPart.Length > 0 ? "," : string.Empty;
                                if (v is string)
                                {
                                    queryPart += "\"" + v + "\"";
                                }
                                else if (v is int || v is Enum)
                                {
                                    queryPart += v;
                                }
                                else
                                {
                                    queryPart += "{";
                                    var isFirst = true;
                                    foreach (var vPropertyInfo in v.GetType().GetProperties())
                                    {
                                        var vValue = vPropertyInfo.GetValue(v);
                                        if (vValue != null)
                                        {
                                            queryPart +=
                                                (isFirst ? "" : ",") + char.ToLowerInvariant(vPropertyInfo.Name[0]) +
                                                vPropertyInfo.Name.Substring(1) + ":" + (vValue is string ? "\"" + vValue + "\"" : vValue);
                                            isFirst = false;
                                        }
                                    }
                                    queryPart += "},";
                                }
                            }

                            queryPart += "]";

                            queryData += queryData.Length > 0 ? "," + queryPart : queryPart;
                        }
                    }
                    else if (type == typeof(ExpandoObject))
                    {
                        var queryPart = char.ToLowerInvariant(propertyInfo.Name[0]) + propertyInfo.Name.Substring(1) + ":";

                        var jsonString = "{";
                        var data = (IDictionary<string, object>)value;
                        foreach (var d in data)
                        {
                            if (jsonString.Length > 1)
                            {
                                jsonString += ",";
                            }
                            var valueQuotes = d.Value is string ? "\"" : string.Empty;

                            var propertyValue = d.Value is bool ? d.Value.ToString().ToLower() : d.Value;

                            jsonString += char.ToLowerInvariant(d.Key[0]) + d.Key.Substring(1) + ":" + valueQuotes + propertyValue + valueQuotes;
                        }
                        jsonString += "}";

                        queryData += queryData.Length > 0 ? "," + queryPart + jsonString : queryPart + jsonString;
                    }
                    else if (type.GetTypeInfo().IsSubclassOf(typeof(GraphQlObject)))
                    {
                        queryData += "," + char.ToLowerInvariant(propertyInfo.Name[0]) + propertyInfo.Name.Substring(1) + ":{";
                        queryData += BuildQueryData(value);
                        queryData += "}";
                    }
                    else
                    {
                        var valueQuotes = type == typeof(string) ? "\"" : string.Empty;

                        var propertyValue = type == typeof(bool) ? value.ToString().ToLower() : value;

                        var queryPart = char.ToLowerInvariant(propertyInfo.Name[0]) +
                                        propertyInfo.Name.Substring(1) + ":" + valueQuotes + propertyValue + valueQuotes;

                        queryData += queryData.Length > 0 ? "," + queryPart : queryPart;
                    }
                }
            }

            return queryData;
        }
    }
