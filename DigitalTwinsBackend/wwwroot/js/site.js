function UpdatePropertyKeyFields() {
    var primitiveDataType = document.getElementById("PrimitiveDataType").value;
    if (primitiveDataType === "Bool") {
        document.getElementById("ValidationDataLabel").textContent = "ValidationData";
        document.getElementById("ValidationData").disabled = "disabled";
        document.getElementById("MinLabel").textContent = "Min";
        document.getElementById("Min").disabled = "disabled";
        document.getElementById("MaxLabel").textContent = "Max";
        document.getElementById("Max").disabled = "disabled";
    }
    if (primitiveDataType === "String") {
        document.getElementById("ValidationDataLabel").innerHTML = "ValidationData - Optional regex <br>> https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference";
        document.getElementById("ValidationData").disabled = "";
        document.getElementById("MinLabel").textContent = "Minimum string length";
        document.getElementById("Min").disabled = "";
        document.getElementById("MaxLabel").textContent = "Maximum string length";
        document.getElementById("Max").disabled = "";
    }
    if (primitiveDataType === "Long" || primitiveDataType === "int" || primitiveDataType === "uint") {
        document.getElementById("ValidationDataLabel").textContent = "ValidationData";
        document.getElementById("ValidationData").disabled = "disabled";
        document.getElementById("MinLabel").textContent = "Minimum allowed value";
        document.getElementById("Min").disabled = "";
        document.getElementById("MaxLabel").textContent = "Maximum allowed value";
        document.getElementById("Max").disabled = "";
    }
    if (primitiveDataType === "DateTime") {
        document.getElementById("ValidationDataLabel").innerHTML = "ValidationData - optional format specifier, for example 'yyyy - MM - dd'. Defaults to 'o' if not specified <br>> https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings";
        document.getElementById("ValidationData").disabled = "";
        document.getElementById("MinLabel").textContent = "Minimum date - ISO8601 format";
        document.getElementById("Min").disabled = "";
        document.getElementById("MaxLabel").textContent = "Maximum date - ISO8601 format";
        document.getElementById("Max").disabled = "";
    }
    if (primitiveDataType === "Set") {
        document.getElementById("ValidationDataLabel").textContent = "ValidationData - val1;val2;…";
        document.getElementById("ValidationData").disabled = "";
        document.getElementById("MinLabel").textContent = "Minimum number of elements";
        document.getElementById("Min").disabled = "";
        document.getElementById("MaxLabel").textContent = "Maximum number of elements";
        document.getElementById("Max").disabled = "";
    }
    if (primitiveDataType === "Enum") {
        document.getElementById("ValidationDataLabel").textContent = "ValidationData - val1;val2;…";
        document.getElementById("ValidationData").disabled = "";
        document.getElementById("MinLabel").textContent = "Min";
        document.getElementById("Min").disabled = "disabled";
        document.getElementById("MaxLabel").textContent = "Max";
        document.getElementById("Max").disabled = "disabled";
    }
    if (primitiveDataType === "Json") {
        document.getElementById("ValidationDataLabel").innerHTML = "ValidationData - optional schema <br>> (http://json-schema.org/)";
        document.getElementById("ValidationData").disabled = "";
        document.getElementById("MinLabel").textContent = "Minimum string length";
        document.getElementById("Min").disabled = "";
        document.getElementById("MaxLabel").textContent = "Maximum string length";
        document.getElementById("Max").disabled = "";
    }
}